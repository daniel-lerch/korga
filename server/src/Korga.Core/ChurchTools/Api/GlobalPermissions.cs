using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Korga.ChurchTools.Api;

public class GlobalPermissions
{
    [JsonPropertyName("churchcore")]
    public required ChurchCorePermissions ChurchCore { get; set; }

    [JsonPropertyName("churchcheckin")]
    public required ChurchCheckinPermissions ChurchCheckin { get; set; }

    [JsonPropertyName("churchdb")]
    public required ChurchDbPermissions ChurchDb { get; set; }

    [JsonPropertyName("churchcal")]
    public required ChurchCalPermissions ChurchCal { get; set; }

    [JsonPropertyName("churchresource")]
    public required ChurchResourcePermissions ChurchResource { get; set; }

    [JsonPropertyName("churchservice")]
    public required ChurchServicePermissions ChurchService { get; set; }

    [JsonPropertyName("churchwiki")]
    public required ChurchWikiPermissions ChurchWiki { get; set; }

    [JsonPropertyName("churchreport")]
    public required ChurchReportPermissions ChurchReport { get; set; }

    [JsonPropertyName("churchgroup")]
    public required ChurchGroupPermissions ChurchGroup { get; set; }


    public class ChurchCorePermissions
    {
        [JsonPropertyName("administer settings")]
        public required bool AdministerSettings { get; set; }

        [JsonPropertyName("edit public profiles")]
        public required bool EditPublicProfiles { get; set; }

        [JsonPropertyName("view website")]
        public required bool ViewWebsite { get; set; }

        [JsonPropertyName("edit website staff")]
        public required bool EditWebsiteStaff { get; set; }

        [JsonPropertyName("edit website releases")]
        public required bool EditWebsiteReleases { get; set; }

        [JsonPropertyName("edit translations masterdata")]
        public required bool EditTranslationsMasterdata { get; set; }

        [JsonPropertyName("edit languages")]
        public required List<int> EditLanguages { get; set; }

        [JsonPropertyName("administer persons")]
        public required bool AdministerPersons { get; set; }

        [JsonPropertyName("view logfile")]
        public required bool ViewLogfile { get; set; }

        [JsonPropertyName("invite persons")]
        public required bool InvitePersons { get; set; }

        [JsonPropertyName("simulate persons")]
        public required bool SimulatePersons { get; set; }
    }

    public class ChurchCheckinPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("create person")]
        public required bool CreatePerson { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }
    }

    public class ChurchDbPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view station")]
        public required List<int> ViewStation { get; set; }

        /// <summary>
        /// List of department IDs from which all people can be seen
        /// </summary>
        [JsonPropertyName("view alldata")]
        public required List<int> ViewAllData { get; set; }

        [JsonPropertyName("security level person")]
        public required List<int> SecurityLevelPerson { get; set; }

        [JsonPropertyName("security level view own data")]
        public required List<int> SecurityLevelViewOwnData { get; set; }

        [JsonPropertyName("security level edit own data")]
        public required List<int> SecurityLevelEditOwnData { get; set; }

        [JsonPropertyName("create person")]
        public required bool CreatePerson { get; set; }

        [JsonPropertyName("write access")]
        public required bool WriteAccess { get; set; }

        [JsonPropertyName("delete persons")]
        public required bool DeletePersons { get; set; }

        [JsonPropertyName("push/pull archive")]
        public required bool PushPullArchive { get; set; }

        [JsonPropertyName("view archive")]
        public required bool ViewArchive { get; set; }

        [JsonPropertyName("view statistics")]
        public required bool ViewStatistics { get; set; }

        [JsonPropertyName("view history")]
        public required bool ViewHistory { get; set; }

        [JsonPropertyName("view birthdaylist")]
        public required bool ViewBirthdaylist { get; set; }

        [JsonPropertyName("view memberliste")]
        public required bool ViewMemberListe { get; set; }

        [JsonPropertyName("view comments")]
        public required List<int> ViewComments { get; set; }

        [JsonPropertyName("view tags")]
        public required bool ViewTags { get; set; }

        [JsonPropertyName("edit relations")]
        public required bool EditRelations { get; set; }

        [JsonPropertyName("complex filter")]
        public required bool ComplexFilter { get; set; }

        [JsonPropertyName("administer global filters")]
        public required bool AdministerGlobalFilters { get; set; }

        [JsonPropertyName("export data")]
        public required bool ExportData { get; set; }

        [JsonPropertyName("edit bulkletter")]
        public required bool EditBulkletter { get; set; }

        [JsonPropertyName("create print labels")]
        public required bool CreatePrintLabels { get; set; }

        [JsonPropertyName("send sms")]
        public required bool SendSms { get; set; }

        [JsonPropertyName("security level group")]
        public required List<int> SecurityLevelGroup { get; set; }

        [JsonPropertyName("view group")]
        public required List<int> ViewGroup { get; set; }

        [JsonPropertyName("edit group")]
        public required List<int> EditGroup { get; set; }

        [JsonPropertyName("delete group")]
        public required List<int> DeleteGroup { get; set; }

        [JsonPropertyName("create groups of grouptype")]
        public required List<int> CreateGroupsOfGrouptype { get; set; }

        [JsonPropertyName("view groups of grouptype")]
        public required List<int> ViewGroupsOfGrouptype { get; set; }

        [JsonPropertyName("edit groups of grouptype")]
        public required List<int> EditGroupsOfGrouptype { get; set; }

        [JsonPropertyName("delete groups of grouptype")]
        public required List<int> DeleteGroupsOfGrouptype { get; set; }

        [JsonPropertyName("edit group memberships")]
        public required bool EditGroupMemberships { get; set; }

        [JsonPropertyName("administer groups")]
        public required bool AdministerGroups { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }

        [JsonPropertyName("edit groups")]
        public required bool EditGroups { get; set; }
    }

    public class ChurchCalPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view category")]
        public required List<int> ViewCategory { get; set; }

        [JsonPropertyName("edit category")]
        public required List<int> EditCategory { get; set; }

        [JsonPropertyName("edit calendar entry template")]
        public required List<int> EditCalendarEntryTemplate { get; set; }

        [JsonPropertyName("assistance mode")]
        public required bool AssistanceMode { get; set; }

        [JsonPropertyName("create personal category")]
        public required bool CreatePersonalCategory { get; set; }

        [JsonPropertyName("admin personal category")]
        public required bool AdminPersonalCategory { get; set; }

        [JsonPropertyName("create group category")]
        public required bool CreateGroupCategory { get; set; }

        [JsonPropertyName("admin group category")]
        public required bool AdminGroupCategory { get; set; }

        [JsonPropertyName("admin church category")]
        public required bool AdminChurchCategory { get; set; }
    }

    public class ChurchResourcePermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view resource")]
        public required List<int> ViewResource { get; set; }

        [JsonPropertyName("create bookings")]
        public required List<int> CreateBookings { get; set; }

        [JsonPropertyName("create virtual bookings")]
        public required bool CreateVirtualBookings { get; set; }

        [JsonPropertyName("administer bookings")]
        public required List<int> AdministerBookings { get; set; }

        [JsonPropertyName("assistance mode")]
        public required bool AssistanceMode { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }
    }

    public class ChurchServicePermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view servicegroup")]
        public required List<int> ViewServiceGroup { get; set; }

        [JsonPropertyName("edit servicegroup")]
        public required List<int> EditServiceGroup { get; set; }

        [JsonPropertyName("view history")]
        public required bool ViewHistory { get; set; }

        [JsonPropertyName("view events")]
        public required List<int> ViewEvents { get; set; }

        [JsonPropertyName("edit events")]
        public required List<int> EditEvents { get; set; }

        [JsonPropertyName("edit template")]
        public required bool EditTemplate { get; set; }

        [JsonPropertyName("manage absent")]
        public required bool ManageAbsent { get; set; }

        [JsonPropertyName("view fact")]
        public required List<int> ViewFact { get; set; }

        [JsonPropertyName("edit fact")]
        public required List<int> EditFact { get; set; }

        [JsonPropertyName("export facts")]
        public required bool ExportFacts { get; set; }

        [JsonPropertyName("view agenda")]
        public required List<int> ViewAgenda { get; set; }

        [JsonPropertyName("edit agenda")]
        public required List<int> EditAgenda { get; set; }

        [JsonPropertyName("edit agenda templates")]
        public required List<int> EditAgendaTemplates { get; set; }

        [JsonPropertyName("view songcategory")]
        public required List<int> ViewSongCategory { get; set; }

        [JsonPropertyName("edit songcategory")]
        public required List<int> EditSongCategory { get; set; }

        [JsonPropertyName("view song statistics")]
        public required bool ViewSongStatistics { get; set; }

        [JsonPropertyName("use ccli")]
        public required bool UseCcli { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }
    }

    public class ChurchWikiPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view category")]
        public required List<int> ViewCategory { get; set; }

        [JsonPropertyName("edit category")]
        public required List<int> EditCategory { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }
    }

    public class ChurchReportPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view query")]
        public required List<int> ViewQuery { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }
    }

    public class ChurchGroupPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }
    }
}
