export interface PersonFilter {
  id: number
  discriminator: string
  statusName: string | null
  groupName: string | null
  groupRoleName: string | null
  groupTypeName: string | null
  personFullName: string | null
}
