import type { Person, Group } from "@/utils/ct-types"
import { churchtoolsClient } from "@churchtools/churchtools-client"
import { z } from "zod"

export type SinglePersonFilter = {
  kind: "person"
  personId: number
}

export type GroupFilter = {
  kind: "group"
  groupId: number
  roleIds: number[]
}

export type PersonFilter = SinglePersonFilter | GroupFilter | null

export type SinglePersonFilterWithNames = SinglePersonFilter & {
  name: string | null
}

export type GroupFilterWithNames = GroupFilter & {
  name: string | null
  roles: (string | null)[]
}

export type PersonFilterWithNames = SinglePersonFilterWithNames | GroupFilterWithNames | null

function varObj(name: string) {
  return z.object({ var: z.literal(name) })
}

function getGroupFilter(filter: unknown[]): PersonFilter {
  const groupFilter = z
    .tuple([
      z.object({ "==": z.tuple([varObj("ctgroup.id"), z.string()]) }),
      z.object({ "==": z.tuple([varObj("groupmember.groupMemberStatus"), z.literal("active")]) }),
    ])
    .rest(z.unknown())
  const groupFilterResult = z.safeParse(groupFilter, filter)
  if (!groupFilterResult.success) {
    return null
  }

  const roleIds: number[] = []

  const remainingFilters = groupFilterResult.data.slice(2)
  if (remainingFilters.length > 0) {
    const groupRoleFilter = z.tuple([
      z.object({ oneof: z.tuple([varObj("role.id"), z.array(z.string())]) }),
    ])
    const groupRoleFilterResult = z.safeParse(groupRoleFilter, remainingFilters)
    if (!groupRoleFilterResult.success) {
      return null
    }

    roleIds.push(...groupRoleFilterResult.data[0].oneof[1].map((id) => parseInt(id)))
  }

  return {
    kind: "group",
    groupId: parseInt(groupFilterResult.data[0]["=="][1]),
    roleIds,
  }
}

function getSinglePersonFilter(filter: unknown[]): PersonFilter {
  const singlePersonFilter = z.tuple([
    z.object({ "==": z.tuple([varObj("person.id"), z.string()]) }),
  ])
  const singlePersonFilterResult = z.safeParse(singlePersonFilter, filter)
  if (!singlePersonFilterResult.success) {
    return null
  }
  return { kind: "person", personId: parseInt(singlePersonFilterResult.data[0]["=="][1]) }
}

function getPersonFilter(filter: unknown[]): PersonFilter {
  const groupFilter = getGroupFilter(filter)
  if (groupFilter !== null) return groupFilter

  return getSinglePersonFilter(filter)
}

export function getMailistFilters(query: unknown): PersonFilter[] | null {
  const topLevelAnd = z.object({
    and: z
      .tuple([
        z.object({ "==": z.tuple([varObj("person.isArchived"), z.literal(0)]) }),
        z.object({ isnull: z.tuple([varObj("person.dateOfDeath")]) }),
      ])
      .rest(z.unknown()),
  })
  const topLevelAndResult = z.safeParse(topLevelAnd, query)
  if (!topLevelAndResult.success) {
    return null
  }

  const remainingFilters = topLevelAndResult.data.and.slice(2)

  const secondLevelOr = z.tuple([
    z.object({
      or: z.array(z.object({ and: z.array(z.unknown()) })),
    }),
  ])
  const secondLevelOrResult = z.safeParse(secondLevelOr, remainingFilters)
  if (secondLevelOrResult.success) {
    const groupFilters = []
    for (const x of secondLevelOrResult.data[0].or) {
      groupFilters.push(getPersonFilter(x.and))
    }
    return groupFilters
  } else {
    const personFilter = getPersonFilter(remainingFilters)
    if (personFilter === null) {
      return null
    }
    return [personFilter]
  }
}

function getPersonName(person?: Person) {
  if (person === undefined) {
    return null
  } else if (person.nickname) {
    return `${person.firstName} (${person.nickname}) ${person.lastName}`
  } else {
    return `${person.firstName} ${person.lastName}`
  }
}

export async function getMailistFiltersWithNames(
  query: unknown
): Promise<PersonFilterWithNames[] | null> {
  const filters = getMailistFilters(query)
  if (filters === null) {
    return null
  }

  const personIds = new Set<number>()
  const groupIds = new Set<number>()
  for (const filter of filters) {
    if (filter?.kind === "person") {
      personIds.add(filter.personId)
    } else if (filter?.kind === "group") {
      groupIds.add(filter.groupId)
    }
  }
  const persons = new Map<number, Person>()
  const groups = new Map<number, Group>()
  if (personIds.size > 0) {
    const personResponse = await churchtoolsClient.get<Person[]>("/persons", {
      ids: Array.from(personIds),
    })
    for (const person of personResponse) {
      persons.set(person.id, person)
    }
  }
  if (groupIds.size > 0) {
    const groupResponse = await churchtoolsClient.get<Group[]>("/groups", {
      ids: Array.from(groupIds),
    })
    for (const group of groupResponse) {
      groups.set(group.id, group)
    }
  }

  const filtersWithNames = []

  for (const filter of filters) {
    if (filter?.kind === "person") {
      filtersWithNames.push({ ...filter, name: getPersonName(persons.get(filter.personId)) })
    } else if (filter?.kind === "group") {
      const group = groups.get(filter.groupId)
      filtersWithNames.push({
        ...filter,
        name: group?.name ?? null,
        roles: filter.roleIds.map(
          (roleId) => group?.roles?.find((role) => role.groupTypeRoleId === roleId)?.name ?? null
        ),
      })
    }
  }

  return filtersWithNames
}
