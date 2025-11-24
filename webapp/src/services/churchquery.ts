import { z } from "zod"

export type PersonFilter =
  | {
      kind: "person"
      personId: number
    }
  | {
      kind: "group"
      groupId: number
      roleIds: number[]
    }
  | null

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
