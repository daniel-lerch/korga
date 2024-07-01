using System;

namespace ChurchTools.Model;

public record GroupMember(
    int PersonId,
    DomainObject Person,
    DomainObject Group,
    int GroupTypeRoleId,
    GroupMemberStatus GroupMemberStatus,
    DateOnly MemberStartDate,
    DateOnly? MemberEndDate,
    string Comment);
