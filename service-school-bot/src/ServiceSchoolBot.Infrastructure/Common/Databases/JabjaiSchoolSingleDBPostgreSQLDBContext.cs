using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

namespace ServiceSchoolBot.Infrastructure.Common.Databases;

public partial class JabjaiSchoolSingleDBPostgreSQLDBContext : DbContext
{
    public JabjaiSchoolSingleDBPostgreSQLDBContext(DbContextOptions<JabjaiSchoolSingleDBPostgreSQLDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActiveSchoolList> ActiveSchoolLists { get; set; }

    public virtual DbSet<Amphur> Amphurs { get; set; }

    public virtual DbSet<Apiconfiguration> Apiconfigurations { get; set; }

    public virtual DbSet<BacklogAutoAssignForwardException> BacklogAutoAssignForwardExceptions { get; set; }

    public virtual DbSet<BacklogAutoAssignRule> BacklogAutoAssignRules { get; set; }

    public virtual DbSet<BacklogGithubRepoMapping> BacklogGithubRepoMappings { get; set; }

    public virtual DbSet<BacklogRootCauseAnalysis> BacklogRootCauseAnalyses { get; set; }

    public virtual DbSet<Bbltransaction> Bbltransactions { get; set; }

    public virtual DbSet<CollectionAlert> CollectionAlerts { get; set; }

    public virtual DbSet<CollectionCallAttempt> CollectionCallAttempts { get; set; }

    public virtual DbSet<CollectionCallRate> CollectionCallRates { get; set; }

    public virtual DbSet<CollectionCampaign> CollectionCampaigns { get; set; }

    public virtual DbSet<CollectionCase> CollectionCases { get; set; }

    public virtual DbSet<CollectionCreditTopup> CollectionCreditTopups { get; set; }

    public virtual DbSet<CollectionCsNote> CollectionCsNotes { get; set; }

    public virtual DbSet<CollectionFallbackMessage> CollectionFallbackMessages { get; set; }

    public virtual DbSet<CollectionPromiseToPay> CollectionPromiseToPays { get; set; }

    public virtual DbSet<CollectionSchoolCredit> CollectionSchoolCredits { get; set; }

    public virtual DbSet<CollectionScript> CollectionScripts { get; set; }

    public virtual DbSet<CollectionSetting> CollectionSettings { get; set; }

    public virtual DbSet<CollectionTranscript> CollectionTranscripts { get; set; }

    public virtual DbSet<CollectionTranscriptAccessLog> CollectionTranscriptAccessLogs { get; set; }

    public virtual DbSet<DeviceDailyStatus> DeviceDailyStatuses { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<ExcelExportDatum> ExcelExportData { get; set; }

    public virtual DbSet<GetPlanCourseTerm> GetPlanCourseTerms { get; set; }

    public virtual DbSet<GsbAuditLogEntry> GsbAuditLogEntries { get; set; }

    public virtual DbSet<GsbMakerUser> GsbMakerUsers { get; set; }

    public virtual DbSet<GsbReportBatch> GsbReportBatches { get; set; }

    public virtual DbSet<GsbSavingsDeposit> GsbSavingsDeposits { get; set; }

    public virtual DbSet<Indexupdated> Indexupdateds { get; set; }

    public virtual DbSet<LessonPlan> LessonPlans { get; set; }

    public virtual DbSet<LessonPlanFile> LessonPlanFiles { get; set; }

    public virtual DbSet<LessonPlanSubmission> LessonPlanSubmissions { get; set; }

    public virtual DbSet<MasterDbtuser> MasterDbtusers { get; set; }

    public virtual DbSet<MasterDbtuserCard> MasterDbtuserCards { get; set; }

    public virtual DbSet<MvSchoolList> MvSchoolLists { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<ResultVerifyUserBalanceBkup> ResultVerifyUserBalanceBkups { get; set; }

    public virtual DbSet<ResultVerifyUserBalanceTempCard> ResultVerifyUserBalanceTempCards { get; set; }

    public virtual DbSet<ResultVerifyUserBalanceTempCardBkup> ResultVerifyUserBalanceTempCardBkups { get; set; }

    public virtual DbSet<SbloginHistory> SbloginHistories { get; set; }

    public virtual DbSet<SchoolDbtemployee> SchoolDbtemployees { get; set; }

    public virtual DbSet<SchoolDbtuser> SchoolDbtusers { get; set; }

    public virtual DbSet<SchoolEmployeeSyncQueue> SchoolEmployeeSyncQueues { get; set; }

    public virtual DbSet<SchoolSetting> SchoolSettings { get; set; }

    public virtual DbSet<SchoolTuser> SchoolTusers { get; set; }

    public virtual DbSet<SchoolUserSyncQueue> SchoolUserSyncQueues { get; set; }

    public virtual DbSet<SearchBySidtemp> SearchBySidtemps { get; set; }

    public virtual DbSet<SearchPlanCourseDetailTemp> SearchPlanCourseDetailTemps { get; set; }

    public virtual DbSet<TFsdqAnswer> TFsdqAnswers { get; set; }

    public virtual DbSet<TFsdqDatum> TFsdqData { get; set; }

    public virtual DbSet<TFsdqPoint> TFsdqPoints { get; set; }

    public virtual DbSet<TFsdqQuestion> TFsdqQuestions { get; set; }

    public virtual DbSet<TapproveEmployeeContactAddress> TapproveEmployeeContactAddresses { get; set; }

    public virtual DbSet<TapproveEmployeeEducation> TapproveEmployeeEducations { get; set; }

    public virtual DbSet<TapproveEmployeeFamily> TapproveEmployeeFamilies { get; set; }

    public virtual DbSet<TapproveEmployeeHistoryReceivingRoyalDecoration> TapproveEmployeeHistoryReceivingRoyalDecorations { get; set; }

    public virtual DbSet<TapproveEmployeeHonor> TapproveEmployeeHonors { get; set; }

    public virtual DbSet<TapproveEmployeeOccupationalLicense> TapproveEmployeeOccupationalLicenses { get; set; }

    public virtual DbSet<TapproveEmployeePermanentAddress> TapproveEmployeePermanentAddresses { get; set; }

    public virtual DbSet<TapproveEmployeeProfile> TapproveEmployeeProfiles { get; set; }

    public virtual DbSet<TapproveEmployeeTraining> TapproveEmployeeTrainings { get; set; }

    public virtual DbSet<TapproveStudentContactAddress> TapproveStudentContactAddresses { get; set; }

    public virtual DbSet<TapproveStudentFatherInfo> TapproveStudentFatherInfos { get; set; }

    public virtual DbSet<TapproveStudentMotherInfo> TapproveStudentMotherInfos { get; set; }

    public virtual DbSet<TapproveStudentParentInfo> TapproveStudentParentInfos { get; set; }

    public virtual DbSet<TapproveStudentPermanentAddress> TapproveStudentPermanentAddresses { get; set; }

    public virtual DbSet<TapproveStudentProfile> TapproveStudentProfiles { get; set; }

    public virtual DbSet<TbCurrentTermDatum> TbCurrentTermData { get; set; }

    public virtual DbSet<TbEqAnswer> TbEqAnswers { get; set; }

    public virtual DbSet<TbEqDatum> TbEqData { get; set; }

    public virtual DbSet<TbEqGroup> TbEqGroups { get; set; }

    public virtual DbSet<TbEqPoint> TbEqPoints { get; set; }

    public virtual DbSet<TbEqQuestion> TbEqQuestions { get; set; }

    public virtual DbSet<TbHistorySetting> TbHistorySettings { get; set; }

    public virtual DbSet<TbSbloginHistoryFiltered> TbSbloginHistoryFiltereds { get; set; }

    public virtual DbSet<TbStudentDetail> TbStudentDetails { get; set; }

    public virtual DbSet<TbStudentView> TbStudentViews { get; set; }

    public virtual DbSet<TbStudentViewsAcademic> TbStudentViewsAcademics { get; set; }

    public virtual DbSet<TbStudentViewsAcademicOpt> TbStudentViewsAcademicOpts { get; set; }

    public virtual DbSet<TbStudentViewsCurrentTerm> TbStudentViewsCurrentTerms { get; set; }

    public virtual DbSet<TbStudentViewsCurrentTermLite> TbStudentViewsCurrentTermLites { get; set; }

    public virtual DbSet<TbStudentViewsCurrentTermLite1> TbStudentViewsCurrentTermLite1s { get; set; }

    public virtual DbSet<TbStudentViewsCurrentTermLite2> TbStudentViewsCurrentTermLite2s { get; set; }

    public virtual DbSet<TbStudentViewsCurrentTermLiteOrig> TbStudentViewsCurrentTermLiteOrigs { get; set; }

    public virtual DbSet<TbStudentViewsFiltered> TbStudentViewsFiltereds { get; set; }

    public virtual DbSet<TbStudentViewsMemory> TbStudentViewsMemories { get; set; }

    public virtual DbSet<TbStudentViewsMemoryOpt> TbStudentViewsMemoryOpts { get; set; }

    public virtual DbSet<TbStudentViewsMemoryOpt1> TbStudentViewsMemoryOpt1s { get; set; }

    public virtual DbSet<TbStudentViewsOptimized> TbStudentViewsOptimizeds { get; set; }

    public virtual DbSet<TbStudentViewsPicture> TbStudentViewsPictures { get; set; }

    public virtual DbSet<TbStudentViewsTest> TbStudentViewsTests { get; set; }

    public virtual DbSet<TbStudentViewsV2> TbStudentViewsV2s { get; set; }

    public virtual DbSet<TbStudentViewsV3> TbStudentViewsV3s { get; set; }

    public virtual DbSet<TbackupCard> TbackupCards { get; set; }

    public virtual DbSet<TbackupCardHistory> TbackupCardHistories { get; set; }

    public virtual DbSet<TblMasterAccidentType> TblMasterAccidentTypes { get; set; }

    public virtual DbSet<TblMasterAllergy> TblMasterAllergies { get; set; }

    public virtual DbSet<TblMasterAllergyType> TblMasterAllergyTypes { get; set; }

    public virtual DbSet<TblMasterDisease> TblMasterDiseases { get; set; }

    public virtual DbSet<TblMasterMedicine> TblMasterMedicines { get; set; }

    public virtual DbSet<TblMasterPriorityLevel> TblMasterPriorityLevels { get; set; }

    public virtual DbSet<TblMasterStatus> TblMasterStatuses { get; set; }

    public virtual DbSet<TblMasterSymptom> TblMasterSymptoms { get; set; }

    public virtual DbSet<TblMasterTreatment> TblMasterTreatments { get; set; }

    public virtual DbSet<TblMasterTreatmentType> TblMasterTreatmentTypes { get; set; }

    public virtual DbSet<TblMasterUnit> TblMasterUnits { get; set; }

    public virtual DbSet<TblMasterVaccine> TblMasterVaccines { get; set; }

    public virtual DbSet<TblMasterVisitType> TblMasterVisitTypes { get; set; }

    public virtual DbSet<TblMedicalProfileAccident> TblMedicalProfileAccidents { get; set; }

    public virtual DbSet<TblMedicalProfileAllergy> TblMedicalProfileAllergies { get; set; }

    public virtual DbSet<TblMedicalProfileChronicDisease> TblMedicalProfileChronicDiseases { get; set; }

    public virtual DbSet<TblMedicalProfileVaccination> TblMedicalProfileVaccinations { get; set; }

    public virtual DbSet<TblMedicalRecord> TblMedicalRecords { get; set; }

    public virtual DbSet<TblMedicalRecordAttachment> TblMedicalRecordAttachments { get; set; }

    public virtual DbSet<TblMedicalRecordMedicine> TblMedicalRecordMedicines { get; set; }

    public virtual DbSet<TblMedicalRecordSymptom> TblMedicalRecordSymptoms { get; set; }

    public virtual DbSet<TblMedicalRecordTreatment> TblMedicalRecordTreatments { get; set; }

    public virtual DbSet<TblMedicineUnitMapping> TblMedicineUnitMappings { get; set; }

    public virtual DbSet<Tbranch> Tbranches { get; set; }

    public virtual DbSet<TbranchSpec> TbranchSpecs { get; set; }

    public virtual DbSet<TbranchSubject> TbranchSubjects { get; set; }

    public virtual DbSet<TbstudentViewSyncQueue> TbstudentViewSyncQueues { get; set; }

    public virtual DbSet<Tclass> Tclasses { get; set; }

    public virtual DbSet<TclassMember> TclassMembers { get; set; }

    public virtual DbSet<TclassOnline> TclassOnlines { get; set; }

    public virtual DbSet<Tcompany> Tcompanies { get; set; }

    public virtual DbSet<Tcompany1> Tcompanies1 { get; set; }

    public virtual DbSet<TcourseAbbreviation> TcourseAbbreviations { get; set; }

    public virtual DbSet<TcourseGroup> TcourseGroups { get; set; }

    public virtual DbSet<TcourseType> TcourseTypes { get; set; }

    public virtual DbSet<Tcurriculum> Tcurricula { get; set; }

    public virtual DbSet<TcurriculumLink> TcurriculumLinks { get; set; }

    public virtual DbSet<TcurriculumLinkCompetencySubject> TcurriculumLinkCompetencySubjects { get; set; }

    public virtual DbSet<TcurriculumLinkCourseType> TcurriculumLinkCourseTypes { get; set; }

    public virtual DbSet<TcurriculumLinkSubjectMapping> TcurriculumLinkSubjectMappings { get; set; }

    public virtual DbSet<Tdepartment> Tdepartments { get; set; }

    public virtual DbSet<TdocumentStampDate> TdocumentStampDates { get; set; }

    public virtual DbSet<TempAddress> TempAddresses { get; set; }

    public virtual DbSet<TempCard> TempCards { get; set; }

    public virtual DbSet<TempEducationInfo> TempEducationInfos { get; set; }

    public virtual DbSet<TempFamily> TempFamilies { get; set; }

    public virtual DbSet<TempHonor> TempHonors { get; set; }

    public virtual DbSet<TempInsignium> TempInsignia { get; set; }

    public virtual DbSet<TempProfessionalLicense> TempProfessionalLicenses { get; set; }

    public virtual DbSet<TempSalary> TempSalaries { get; set; }

    public virtual DbSet<TempSigner> TempSigners { get; set; }

    public virtual DbSet<TempTeaching> TempTeachings { get; set; }

    public virtual DbSet<TempToeic> TempToeics { get; set; }

    public virtual DbSet<TempTraining> TempTrainings { get; set; }

    public virtual DbSet<Temployee> Temployees { get; set; }

    public virtual DbSet<TemployeeInfo> TemployeeInfos { get; set; }

    public virtual DbSet<TemployeeType> TemployeeTypes { get; set; }

    public virtual DbSet<TfamilyProfile> TfamilyProfiles { get; set; }

    public virtual DbSet<TfeatureSbplu> TfeatureSbplus { get; set; }

    public virtual DbSet<TgradeAttendance> TgradeAttendances { get; set; }

    public virtual DbSet<TgradeCheck> TgradeChecks { get; set; }

    public virtual DbSet<TgradeLock> TgradeLocks { get; set; }

    public virtual DbSet<TgradeMigration> TgradeMigrations { get; set; }

    public virtual DbSet<TgradeRegisterPeriod> TgradeRegisterPeriods { get; set; }

    public virtual DbSet<TgradeShareInfo> TgradeShareInfos { get; set; }

    public virtual DbSet<TgradeTeacherDescribe> TgradeTeacherDescribes { get; set; }

    public virtual DbSet<TgradeViewRoomListSetting> TgradeViewRoomListSettings { get; set; }

    public virtual DbSet<TgradeViewSetting> TgradeViewSettings { get; set; }

    public virtual DbSet<TgradeViewStudentBlockListSetting> TgradeViewStudentBlockListSettings { get; set; }

    public virtual DbSet<ThealtProfile> ThealtProfiles { get; set; }

    public virtual DbSet<ThistoryLog> ThistoryLogs { get; set; }

    public virtual DbSet<Tholiday> Tholidays { get; set; }

    public virtual DbSet<TholidayLog> TholidayLogs { get; set; }

    public virtual DbSet<TholidaySome> TholidaySomes { get; set; }

    public virtual DbSet<ThomeWorkFile> ThomeWorkFiles { get; set; }

    public virtual DbSet<ThomeWorkLearning> ThomeWorkLearnings { get; set; }

    public virtual DbSet<ThomeWorkLearningFile> ThomeWorkLearningFiles { get; set; }

    public virtual DbSet<ThomeWorkReply> ThomeWorkReplies { get; set; }

    public virtual DbSet<ThomeWorkReplyFile> ThomeWorkReplyFiles { get; set; }

    public virtual DbSet<Thomework> Thomeworks { get; set; }

    public virtual DbSet<ThomeworkUser> ThomeworkUsers { get; set; }

    public virtual DbSet<ThomeworkUserFile> ThomeworkUserFiles { get; set; }

    public virtual DbSet<TjobList> TjobLists { get; set; }

    public virtual DbSet<TlearningCenter> TlearningCenters { get; set; }

    public virtual DbSet<Tlevel> Tlevels { get; set; }

    public virtual DbSet<TmasterDatum> TmasterData { get; set; }

    public virtual DbSet<TmasterDbuser> TmasterDbusers { get; set; }

    public virtual DbSet<TmasterType> TmasterTypes { get; set; }

    public virtual DbSet<TmessageLinemulticast> TmessageLinemulticasts { get; set; }

    public virtual DbSet<TnewsFile> TnewsFiles { get; set; }

    public virtual DbSet<Tonetscore> Tonetscores { get; set; }

    public virtual DbSet<Tparent> Tparents { get; set; }

    public virtual DbSet<TparentStudent> TparentStudents { get; set; }

    public virtual DbSet<Tperiod> Tperiods { get; set; }

    public virtual DbSet<TperiodTsubLevel> TperiodTsubLevels { get; set; }

    public virtual DbSet<TpermissionCard> TpermissionCards { get; set; }

    public virtual DbSet<TpermissionCardType> TpermissionCardTypes { get; set; }

    public virtual DbSet<Tplan> Tplans { get; set; }

    public virtual DbSet<TplanCourse> TplanCourses { get; set; }

    public virtual DbSet<TplanCourseAdjustedTerm> TplanCourseAdjustedTerms { get; set; }

    public virtual DbSet<TplanCourseAssessmentTopic> TplanCourseAssessmentTopics { get; set; }

    public virtual DbSet<TplanCourseSection> TplanCourseSections { get; set; }

    public virtual DbSet<TplanCourseSectionAllowedRoom> TplanCourseSectionAllowedRooms { get; set; }

    public virtual DbSet<TplanCourseSectionEnrollment> TplanCourseSectionEnrollments { get; set; }

    public virtual DbSet<TplanCourseSectionPeriod> TplanCourseSectionPeriods { get; set; }

    public virtual DbSet<TplanCourseSetting> TplanCourseSettings { get; set; }

    public virtual DbSet<TplanCourseStudent> TplanCourseStudents { get; set; }

    public virtual DbSet<TplanCourseTeacher> TplanCourseTeachers { get; set; }

    public virtual DbSet<TplanCourseTerm> TplanCourseTerms { get; set; }

    public virtual DbSet<TplanFeature> TplanFeatures { get; set; }

    public virtual DbSet<TplanOwner> TplanOwners { get; set; }

    public virtual DbSet<TplanTermSubLevel2> TplanTermSubLevel2s { get; set; }

    public virtual DbSet<Tplane> Tplanes { get; set; }

    public virtual DbSet<TpreRegister> TpreRegisters { get; set; }

    public virtual DbSet<TpreRegisterDocument> TpreRegisterDocuments { get; set; }

    public virtual DbSet<TpreRegisterFieldPageSetting> TpreRegisterFieldPageSettings { get; set; }

    public virtual DbSet<TpreRegisterRequiredCategory> TpreRegisterRequiredCategories { get; set; }

    public virtual DbSet<TpreRegisterRequiredField> TpreRegisterRequiredFields { get; set; }

    public virtual DbSet<TpreRegisterRequiredFieldInitiate> TpreRegisterRequiredFieldInitiates { get; set; }

    public virtual DbSet<TpreRegisterSendMail> TpreRegisterSendMails { get; set; }

    public virtual DbSet<TpreRegisterSendMailAttachment> TpreRegisterSendMailAttachments { get; set; }

    public virtual DbSet<TpreRegisterSurveyAnswer> TpreRegisterSurveyAnswers { get; set; }

    public virtual DbSet<TpreRegisterSurveyOption> TpreRegisterSurveyOptions { get; set; }

    public virtual DbSet<TpreRegisterSurveyTopic> TpreRegisterSurveyTopics { get; set; }

    public virtual DbSet<TpromoteBatchDetail> TpromoteBatchDetails { get; set; }

    public virtual DbSet<TrackingDatum> TrackingData { get; set; }

    public virtual DbSet<TregisterExamRoom> TregisterExamRooms { get; set; }

    public virtual DbSet<TregisterExplanation> TregisterExplanations { get; set; }

    public virtual DbSet<TregisterPlanSetup> TregisterPlanSetups { get; set; }

    public virtual DbSet<TregisterProgramSetup> TregisterProgramSetups { get; set; }

    public virtual DbSet<TregisterRegularity> TregisterRegularities { get; set; }

    public virtual DbSet<TregisterSetup> TregisterSetups { get; set; }

    public virtual DbSet<Troom> Trooms { get; set; }

    public virtual DbSet<TroomChange> TroomChanges { get; set; }

    public virtual DbSet<Tschedule> Tschedules { get; set; }

    public virtual DbSet<TscheduleTeacher> TscheduleTeachers { get; set; }

    public virtual DbSet<TschoolIntegration> TschoolIntegrations { get; set; }

    public virtual DbSet<TschoolRecord> TschoolRecords { get; set; }

    public virtual DbSet<TschoolRecordDetail> TschoolRecordDetails { get; set; }

    public virtual DbSet<TsendDataPsi> TsendDataPses { get; set; }

    public virtual DbSet<TsendGradePsi> TsendGradePses { get; set; }

    public virtual DbSet<TsendSubjectPsi> TsendSubjectPses { get; set; }

    public virtual DbSet<TsettingExtraTime> TsettingExtraTimes { get; set; }

    public virtual DbSet<Tsm> Tsms { get; set; }

    public virtual DbSet<Tsmsgroup> Tsmsgroups { get; set; }

    public virtual DbSet<TsmsgroupUser> TsmsgroupUsers { get; set; }

    public virtual DbSet<TsmssubLevel> TsmssubLevels { get; set; }

    public virtual DbSet<TstudentCardInfo> TstudentCardInfos { get; set; }

    public virtual DbSet<TstudentClassroomHistory> TstudentClassroomHistories { get; set; }

    public virtual DbSet<TstudentDisability> TstudentDisabilities { get; set; }

    public virtual DbSet<TstudentHealthGrowth> TstudentHealthGrowths { get; set; }

    public virtual DbSet<TstudentHealthInfo> TstudentHealthInfos { get; set; }

    public virtual DbSet<TstudentHistory> TstudentHistories { get; set; }

    public virtual DbSet<TstudentLevel> TstudentLevels { get; set; }

    public virtual DbSet<TstudentType> TstudentTypes { get; set; }

    public virtual DbSet<TsubLevel> TsubLevels { get; set; }

    public virtual DbSet<TsubcriptionInvoice> TsubcriptionInvoices { get; set; }

    public virtual DbSet<TsubcriptionPlan> TsubcriptionPlans { get; set; }

    public virtual DbSet<Tsubscription> Tsubscriptions { get; set; }

    public virtual DbSet<TsubscriptionIdempotencyKey> TsubscriptionIdempotencyKeys { get; set; }

    public virtual DbSet<TteacherCardInfo> TteacherCardInfos { get; set; }

    public virtual DbSet<TteacherStudentType> TteacherStudentTypes { get; set; }

    public virtual DbSet<TtempScanCameraIpstatus> TtempScanCameraIpstatuses { get; set; }

    public virtual DbSet<TtempScanStatusOnline> TtempScanStatusOnlines { get; set; }

    public virtual DbSet<Tterm> Tterms { get; set; }

    public virtual DbSet<TtermSubLevel2> TtermSubLevel2s { get; set; }

    public virtual DbSet<TtermTimeTable> TtermTimeTables { get; set; }

    public virtual DbSet<Ttime> Ttimes { get; set; }

    public virtual DbSet<TtimetableProcessResult> TtimetableProcessResults { get; set; }

    public virtual DbSet<Ttimetype> Ttimetypes { get; set; }

    public virtual DbSet<TtitleList> TtitleLists { get; set; }

    public virtual DbSet<TtranscriptNumber> TtranscriptNumbers { get; set; }

    public virtual DbSet<Ttype> Ttypes { get; set; }

    public virtual DbSet<Tuser> Tusers { get; set; }

    public virtual DbSet<TuserAttendance> TuserAttendances { get; set; }

    public virtual DbSet<TuserCard> TuserCards { get; set; }

    public virtual DbSet<TuserVerify> TuserVerifies { get; set; }

    public virtual DbSet<TuserVerifyLineconnect> TuserVerifyLineconnects { get; set; }

    public virtual DbSet<Tyear> Tyears { get; set; }

    public virtual DbSet<UpdateBalance> UpdateBalances { get; set; }

    public virtual DbSet<UpdateBalanceLog> UpdateBalanceLogs { get; set; }

    public virtual DbSet<UpdateBalanceMemory> UpdateBalanceMemories { get; set; }

    public virtual DbSet<UpdateBalanceMemoryBackup> UpdateBalanceMemoryBackups { get; set; }

    public virtual DbSet<UserAuditTable> UserAuditTables { get; set; }

    public virtual DbSet<UserDailyBalance> UserDailyBalances { get; set; }

    public virtual DbSet<UserDailyBalanceBoat> UserDailyBalanceBoats { get; set; }

    public virtual DbSet<UserDailyBalanceTempCard> UserDailyBalanceTempCards { get; set; }

    public virtual DbSet<UserDailyBalanceTempCardBoat> UserDailyBalanceTempCardBoats { get; set; }

    public virtual DbSet<UserUsedLimit> UserUsedLimits { get; set; }

    public virtual DbSet<VStudentInfo> VStudentInfos { get; set; }

    public virtual DbSet<VUserList> VUserLists { get; set; }

    public virtual DbSet<VgetAllStudentInfoByRoom> VgetAllStudentInfoByRooms { get; set; }

    public virtual DbSet<VgetPlanRoomList> VgetPlanRoomLists { get; set; }

    public virtual DbSet<ViewActiveSchoolList> ViewActiveSchoolLists { get; set; }

    public virtual DbSet<ViewTopupMemoryDataNew> ViewTopupMemoryDataNews { get; set; }

    public virtual DbSet<ViewTopupMemoryDataOptimized> ViewTopupMemoryDataOptimizeds { get; set; }

    public virtual DbSet<ViewTopupMemoryDataOptimized1> ViewTopupMemoryDataOptimized1s { get; set; }

    public virtual DbSet<ViewTopupMemoryDataRedi> ViewTopupMemoryDataRedis { get; set; }

    public virtual DbSet<ViewTopupMemoryDataSyncQueue> ViewTopupMemoryDataSyncQueues { get; set; }

    public virtual DbSet<ViewTopupMemoryDatum> ViewTopupMemoryData { get; set; }

    public virtual DbSet<VreturnedTempCard> VreturnedTempCards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("dblink")
            .HasPostgresExtension("pg_stat_statements")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgaudit")
            .HasPostgresExtension("postgres_fdw")
            .HasPostgresExtension("rds_pg_sql_ccl");

        modelBuilder.Entity<ActiveSchoolList>(entity =>
        {
            entity.HasKey(e => e.NCompany).HasName("ActiveSchoolList_pkey");

            entity.ToTable("ActiveSchoolList");

            entity.HasIndex(e => e.NCompany, "IDX_nCompany").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.NCompany)
                .ValueGeneratedNever()
                .HasColumnName("nCompany");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.SCompany)
                .HasMaxLength(250)
                .HasColumnName("sCompany");
        });

        modelBuilder.Entity<Amphur>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("amphur");

            entity.Property(e => e.AmphurCode)
                .HasMaxLength(4)
                .HasColumnName("AMPHUR_CODE");
            entity.Property(e => e.AmphurId).HasColumnName("AMPHUR_ID");
            entity.Property(e => e.AmphurName)
                .HasMaxLength(150)
                .HasColumnName("AMPHUR_NAME");
            entity.Property(e => e.AmphurNameEn)
                .HasMaxLength(150)
                .HasColumnName("AMPHUR_NAME_EN");
            entity.Property(e => e.GeoId).HasColumnName("GEO_ID");
            entity.Property(e => e.Postcode)
                .HasMaxLength(5)
                .HasColumnName("POSTCODE");
            entity.Property(e => e.ProvinceId).HasColumnName("PROVINCE_ID");
        });

        modelBuilder.Entity<Apiconfiguration>(entity =>
        {
            entity.HasKey(e => e.ConfigurationId).HasName("public_APIConfiguration_PK_APIConfiguration");

            entity.ToTable("APIConfiguration");

            entity.Property(e => e.ConfigurationId).HasColumnName("ConfigurationID");
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<BacklogAutoAssignForwardException>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BacklogAutoAssignForwardException_pkey");

            entity.ToTable("BacklogAutoAssignForwardException");

            entity.HasIndex(e => e.BacklogUserId, "uq_backlogautoassignforwardexception_userid").IsUnique();

            entity.Property(e => e.BacklogMailAddress).HasMaxLength(200);
            entity.Property(e => e.BacklogUserName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<BacklogAutoAssignRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BacklogAutoAssignRule_pkey");

            entity.ToTable("BacklogAutoAssignRule");

            entity.HasIndex(e => e.BacklogProjectId, "uq_backlogautoassignrule_projectid").IsUnique();

            entity.Property(e => e.AssigneeMailAddress).HasMaxLength(200);
            entity.Property(e => e.AssigneeName).HasMaxLength(200);
            entity.Property(e => e.BacklogProjectKey).HasMaxLength(50);
            entity.Property(e => e.BacklogProjectName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<BacklogGithubRepoMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BacklogGithubRepoMapping_pkey");

            entity.ToTable("BacklogGithubRepoMapping");

            entity.HasIndex(e => e.BacklogProjectId, "idx_backloggithubrepomapping_backlogprojectid");

            entity.HasIndex(e => new { e.BacklogProjectId, e.GithubOwner, e.GithubRepo }, "uq_backloggithubrepomapping_project_repo").IsUnique();

            entity.Property(e => e.BacklogProjectKey).HasMaxLength(50);
            entity.Property(e => e.BacklogProjectName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DefaultBranch)
                .HasMaxLength(100)
                .HasDefaultValueSql("'main'::character varying");
            entity.Property(e => e.GithubOwner).HasMaxLength(100);
            entity.Property(e => e.GithubRepo).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RepoNote).HasMaxLength(200);
            entity.Property(e => e.RepoRole).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<BacklogRootCauseAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BacklogRootCauseAnalysis_pkey");

            entity.ToTable("BacklogRootCauseAnalysis");

            entity.HasIndex(e => e.IssueKey, "idx_backlogrootcauseanalysis_issuekey");

            entity.HasIndex(e => new { e.BacklogProjectId, e.CreatedAt }, "idx_backlogrootcauseanalysis_project_createdat");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.IssueKey).HasMaxLength(50);
            entity.Property(e => e.IssueSummary).HasMaxLength(500);
            entity.Property(e => e.ModelUsed).HasMaxLength(50);
        });

        modelBuilder.Entity<Bbltransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("public_BBLTransaction_PK_BBLTransaction");

            entity.ToTable("BBLTransaction");

            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("TransactionID");
            entity.Property(e => e.Amount)
                .HasMaxLength(100)
                .HasColumnName("amount");
            entity.Property(e => e.Channel)
                .HasMaxLength(100)
                .HasColumnName("channel");
            entity.Property(e => e.ErrorCode).HasMaxLength(250);
            entity.Property(e => e.ErrorMessage).HasMaxLength(250);
            entity.Property(e => e.FromBank)
                .HasMaxLength(100)
                .HasColumnName("fromBank");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.NotifyCode).HasMaxLength(10);
            entity.Property(e => e.NotifyDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NotifyStatus).HasMaxLength(100);
            entity.Property(e => e.PayeeId)
                .HasMaxLength(100)
                .HasColumnName("payeeId");
            entity.Property(e => e.Reference1)
                .HasMaxLength(100)
                .HasColumnName("reference1");
            entity.Property(e => e.Reference2)
                .HasMaxLength(100)
                .HasColumnName("reference2");
            entity.Property(e => e.RetryFlag)
                .HasMaxLength(100)
                .HasColumnName("retryFlag");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TermId)
                .HasMaxLength(100)
                .HasColumnName("termId");
            entity.Property(e => e.TransDate)
                .HasMaxLength(100)
                .HasColumnName("transDate");
            entity.Property(e => e.TransRef)
                .HasMaxLength(100)
                .HasColumnName("transRef");
            entity.Property(e => e.TransTime)
                .HasMaxLength(100)
                .HasColumnName("transTime");
            entity.Property(e => e.VerifyCode).HasMaxLength(10);
            entity.Property(e => e.VerifyDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.VerifyStatus).HasMaxLength(100);
        });

        modelBuilder.Entity<CollectionAlert>(entity =>
        {
            entity.HasKey(e => e.AlertId).HasName("CollectionAlert_pkey");

            entity.ToTable("CollectionAlert");

            entity.HasIndex(e => e.CaseId, "ix_fk_11630781_collectionalert");

            entity.HasIndex(e => new { e.SchoolId, e.AcknowledgedAt }, "public_CollectionAlert_IX_SchoolID_AcknowledgedAt");

            entity.Property(e => e.AlertId)
                .ValueGeneratedNever()
                .HasColumnName("AlertID");
            entity.Property(e => e.AcknowledgedAt).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CaseId).HasColumnName("CaseID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Detail).HasMaxLength(500);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Type).HasMaxLength(30);

            entity.HasOne(d => d.Case).WithMany(p => p.CollectionAlerts)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("CollectionAlert_CaseID_fkey");
        });

        modelBuilder.Entity<CollectionCallAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("CollectionCallAttempt_pkey");

            entity.ToTable("CollectionCallAttempt");

            entity.HasIndex(e => e.CampaignId, "ix_fk_11630761_collectioncallattempt");

            entity.HasIndex(e => e.CaseId, "public_CollectionCallAttempt_IX_CaseID");

            entity.HasIndex(e => e.Outcome, "public_CollectionCallAttempt_IX_Outcome");

            entity.HasIndex(e => e.AladinCallId, "public_CollectionCallAttempt_UQ_AladinCallID").IsUnique();

            entity.Property(e => e.AttemptId)
                .ValueGeneratedNever()
                .HasColumnName("AttemptID");
            entity.Property(e => e.AladinCallId)
                .HasMaxLength(100)
                .HasColumnName("AladinCallID");
            entity.Property(e => e.BillableEvent).HasMaxLength(20);
            entity.Property(e => e.BilledAmount).HasPrecision(10, 2);
            entity.Property(e => e.CalledAt).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.CaseId).HasColumnName("CaseID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CreditBalanceAfter).HasPrecision(12, 2);
            entity.Property(e => e.MinutesBilled).HasPrecision(10, 2);
            entity.Property(e => e.Outcome).HasMaxLength(20);
            entity.Property(e => e.RateApplied).HasPrecision(10, 2);
            entity.Property(e => e.RawWebhook).HasColumnType("jsonb");
            entity.Property(e => e.RecordingUrl).HasMaxLength(500);
            entity.Property(e => e.ResultIntent).HasMaxLength(50);
            entity.Property(e => e.ScheduledAt).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Stage).HasMaxLength(20);

            entity.HasOne(d => d.Campaign).WithMany(p => p.CollectionCallAttempts)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("CollectionCallAttempt_CampaignID_fkey");

            entity.HasOne(d => d.Case).WithMany(p => p.CollectionCallAttempts)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("CollectionCallAttempt_CaseID_fkey");
        });

        modelBuilder.Entity<CollectionCallRate>(entity =>
        {
            entity.HasKey(e => e.RateId).HasName("CollectionCallRate_pkey");

            entity.ToTable("CollectionCallRate");

            entity.HasIndex(e => new { e.SchoolId, e.EffectiveFrom }, "public_CollectionCallRate_IX_SchoolID_EffectiveFrom");

            entity.Property(e => e.RateId)
                .ValueGeneratedNever()
                .HasColumnName("RateID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EffectiveFrom).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EffectiveTo).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Note).HasMaxLength(300);
            entity.Property(e => e.RatePerCall).HasPrecision(10, 2);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<CollectionCampaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("CollectionCampaign_pkey");

            entity.ToTable("CollectionCampaign");

            entity.HasIndex(e => new { e.SchoolId, e.Status }, "public_CollectionCampaign_IX_SchoolID_Status");

            entity.Property(e => e.CampaignId)
                .ValueGeneratedNever()
                .HasColumnName("CampaignID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PlaybookId)
                .HasMaxLength(100)
                .HasColumnName("PlaybookID");
            entity.Property(e => e.ScheduledAt).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Stage).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TargetFilter).HasColumnType("jsonb");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<CollectionCase>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("CollectionCase_pkey");

            entity.ToTable("CollectionCase");

            entity.HasIndex(e => e.DueDate, "public_CollectionCase_IX_DueDate");

            entity.HasIndex(e => new { e.SchoolId, e.Status }, "public_CollectionCase_IX_SchoolID_Status");

            entity.HasIndex(e => new { e.SchoolId, e.StudentCode, e.AcademicYear, e.Term }, "public_CollectionCase_UQ_School_Student_Year_Term").IsUnique();

            entity.Property(e => e.CaseId)
                .ValueGeneratedNever()
                .HasColumnName("CaseID");
            entity.Property(e => e.AcademicYear).HasMaxLength(10);
            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DueDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ExcludeReason).HasMaxLength(300);
            entity.Property(e => e.GuardianEmail).HasMaxLength(200);
            entity.Property(e => e.GuardianName).HasMaxLength(200);
            entity.Property(e => e.GuardianPhone).HasMaxLength(20);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.LastSyncedAt).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LevelName).HasMaxLength(100);
            entity.Property(e => e.OutstandingAmount).HasPrecision(18, 2);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Stage).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.StudentCode).HasMaxLength(50);
            entity.Property(e => e.StudentName).HasMaxLength(200);
            entity.Property(e => e.Term).HasMaxLength(10);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<CollectionCreditTopup>(entity =>
        {
            entity.HasKey(e => e.TopupId).HasName("CollectionCreditTopup_pkey");

            entity.ToTable("CollectionCreditTopup");

            entity.HasIndex(e => new { e.SchoolId, e.CreateDate }, "public_CollectionCreditTopup_IX_SchoolID_CreateDate");

            entity.Property(e => e.TopupId)
                .ValueGeneratedNever()
                .HasColumnName("TopupID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EffectiveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.MinutesPurchased).HasPrecision(12, 2);
            entity.Property(e => e.Note).HasMaxLength(300);
            entity.Property(e => e.PeakDocumentUrl).HasMaxLength(500);
            entity.Property(e => e.PeakInvoiceId).HasMaxLength(100);
            entity.Property(e => e.PeakInvoiceNo).HasMaxLength(50);
            entity.Property(e => e.PeakSyncStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
        });

        modelBuilder.Entity<CollectionCsNote>(entity =>
        {
            entity.HasKey(e => e.NoteId).HasName("CollectionCsNote_pkey");

            entity.ToTable("CollectionCsNote");

            entity.HasIndex(e => e.CaseId, "public_CollectionCsNote_IX_CaseID");

            entity.Property(e => e.NoteId)
                .ValueGeneratedNever()
                .HasColumnName("NoteID");
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.CaseId).HasColumnName("CaseID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Case).WithMany(p => p.CollectionCsNotes)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("CollectionCsNote_CaseID_fkey");
        });

        modelBuilder.Entity<CollectionFallbackMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("CollectionFallbackMessage_pkey");

            entity.ToTable("CollectionFallbackMessage");

            entity.HasIndex(e => e.CaseId, "public_CollectionFallbackMessage_IX_CaseID");

            entity.Property(e => e.MessageId)
                .ValueGeneratedNever()
                .HasColumnName("MessageID");
            entity.Property(e => e.CaseId).HasColumnName("CaseID");
            entity.Property(e => e.Channel).HasMaxLength(10);
            entity.Property(e => e.DeliveryStatus).HasMaxLength(20);
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Case).WithMany(p => p.CollectionFallbackMessages)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("CollectionFallbackMessage_CaseID_fkey");
        });

        modelBuilder.Entity<CollectionPromiseToPay>(entity =>
        {
            entity.HasKey(e => e.PromiseId).HasName("CollectionPromiseToPay_pkey");

            entity.ToTable("CollectionPromiseToPay");

            entity.HasIndex(e => e.CaseId, "ix_fk_11630771_collectionpromisetopay");

            entity.HasIndex(e => new { e.PromiseDate, e.Result }, "public_CollectionPromiseToPay_IX_PromiseDate_Result");

            entity.Property(e => e.PromiseId)
                .ValueGeneratedNever()
                .HasColumnName("PromiseID");
            entity.Property(e => e.AttemptId).HasColumnName("AttemptID");
            entity.Property(e => e.CaseId).HasColumnName("CaseID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.PromiseDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.PromisedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Result).HasMaxLength(20);
            entity.Property(e => e.VerifiedAt).HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Case).WithMany(p => p.CollectionPromiseToPays)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("CollectionPromiseToPay_CaseID_fkey");
        });

        modelBuilder.Entity<CollectionSchoolCredit>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("CollectionSchoolCredit_pkey");

            entity.ToTable("CollectionSchoolCredit");

            entity.HasIndex(e => e.BalanceMinutes, "public_CollectionSchoolCredit_IX_BalanceMinutes");

            entity.Property(e => e.SchoolId)
                .ValueGeneratedNever()
                .HasColumnName("SchoolID");
            entity.Property(e => e.BalanceMinutes).HasPrecision(12, 2);
            entity.Property(e => e.LowBalanceThresholdMinutes).HasPrecision(12, 2);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<CollectionScript>(entity =>
        {
            entity.HasKey(e => e.ScriptId).HasName("CollectionScript_pkey");

            entity.ToTable("CollectionScript");

            entity.HasIndex(e => new { e.SchoolId, e.Stage, e.Version }, "public_CollectionScript_UQ_SchoolID_Stage_Version").IsUnique();

            entity.Property(e => e.ScriptId)
                .ValueGeneratedNever()
                .HasColumnName("ScriptID");
            entity.Property(e => e.ApprovedAt).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Stage).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Version).HasDefaultValue(1);
        });

        modelBuilder.Entity<CollectionSetting>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("CollectionSetting_pkey");

            entity.ToTable("CollectionSetting");

            entity.Property(e => e.SchoolId)
                .ValueGeneratedNever()
                .HasColumnName("SchoolID");
            entity.Property(e => e.AladinPlaybookMap).HasColumnType("jsonb");
            entity.Property(e => e.CallWindowEnd).HasMaxLength(5);
            entity.Property(e => e.CallWindowStart).HasMaxLength(5);
            entity.Property(e => e.FallbackChannels).HasColumnType("jsonb");
            entity.Property(e => e.MaxRetry).HasDefaultValue(3);
            entity.Property(e => e.RetryGapHours).HasDefaultValue(3);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<CollectionTranscript>(entity =>
        {
            entity.HasKey(e => e.TranscriptId).HasName("CollectionTranscript_pkey");

            entity.ToTable("CollectionTranscript");

            entity.HasIndex(e => new { e.AttemptId, e.Seq }, "public_CollectionTranscript_UQ_AttemptID_Seq").IsUnique();

            entity.Property(e => e.TranscriptId)
                .ValueGeneratedNever()
                .HasColumnName("TranscriptID");
            entity.Property(e => e.AttemptId).HasColumnName("AttemptID");
            entity.Property(e => e.EndedAtSec).HasPrecision(10, 2);
            entity.Property(e => e.Speaker).HasMaxLength(10);
            entity.Property(e => e.StartedAtSec).HasPrecision(10, 2);

            entity.HasOne(d => d.Attempt).WithMany(p => p.CollectionTranscripts)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("CollectionTranscript_AttemptID_fkey");
        });

        modelBuilder.Entity<CollectionTranscriptAccessLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("CollectionTranscriptAccessLog_pkey");

            entity.ToTable("CollectionTranscriptAccessLog");

            entity.HasIndex(e => e.AttemptId, "public_CollectionTranscriptAccessLog_IX_AttemptID");

            entity.Property(e => e.LogId)
                .ValueGeneratedNever()
                .HasColumnName("LogID");
            entity.Property(e => e.AccessedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.AttemptId).HasColumnName("AttemptID");

            entity.HasOne(d => d.Attempt).WithMany(p => p.CollectionTranscriptAccessLogs)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("CollectionTranscriptAccessLog_AttemptID_fkey");
        });

        modelBuilder.Entity<DeviceDailyStatus>(entity =>
        {
            entity.HasKey(e => e.DeviceStatusId).HasName("public_DeviceDailyStatus_PK_DeviceDailyStatus_new");

            entity.ToTable("DeviceDailyStatus");

            entity.HasIndex(e => new { e.SchoolId, e.OnlineTime }, "public_DeviceDailyStatus_INDEX_VV_3735_3734_new");

            entity.HasIndex(e => new { e.Online, e.Login }, "public_DeviceDailyStatus_INDEX_VV_629_628");

            entity.HasIndex(e => new { e.SchoolId, e.DeviceId, e.BusinessDate }, "public_DeviceDailyStatus_IX_DeviceDailyStatus_SchoolID_DeviceID");

            entity.Property(e => e.DeviceStatusId)
                .ValueGeneratedNever()
                .HasColumnName("DeviceStatusID");
            entity.Property(e => e.AppName).HasMaxLength(200);
            entity.Property(e => e.AppVersion).HasMaxLength(200);
            entity.Property(e => e.BusinessDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.DeviceTypeId).HasColumnName("DeviceTypeID");
            entity.Property(e => e.LoginTime).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LogoutTime).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NotifyEnabled).HasDefaultValue(true);
            entity.Property(e => e.OnlineTime).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UserLoginId).HasColumnName("UserLoginID");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasKey(e => e.DeviceTypeId).HasName("public_DeviceType_PK_DeviceType");

            entity.ToTable("DeviceType");

            entity.Property(e => e.DeviceTypeId)
                .ValueGeneratedNever()
                .HasColumnName("DeviceTypeID");
            entity.Property(e => e.TypeNameEn)
                .HasMaxLength(250)
                .HasColumnName("TypeNameEN");
            entity.Property(e => e.TypeNameTh)
                .HasMaxLength(250)
                .HasColumnName("TypeNameTH");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("district");

            entity.Property(e => e.AmphurId).HasColumnName("AMPHUR_ID");
            entity.Property(e => e.DistrictCode)
                .HasMaxLength(6)
                .HasColumnName("DISTRICT_CODE");
            entity.Property(e => e.DistrictId).HasColumnName("DISTRICT_ID");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(150)
                .HasColumnName("DISTRICT_NAME");
            entity.Property(e => e.DistrictNameEn)
                .HasMaxLength(150)
                .HasColumnName("DISTRICT_NAME_EN");
            entity.Property(e => e.GeoId).HasColumnName("GEO_ID");
            entity.Property(e => e.ProvinceId).HasColumnName("PROVINCE_ID");
        });

        modelBuilder.Entity<ExcelExportDatum>(entity =>
        {
            entity.HasKey(e => e.ExportId).HasName("public_ExcelExportData_PK_ExcelExportData");

            entity.Property(e => e.ExportId).HasColumnName("ExportID");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 2)
                .HasColumnName("nMoney");
            entity.Property(e => e.Name).HasMaxLength(300);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Status).HasMaxLength(1);
            entity.Property(e => e.StudentId)
                .HasMaxLength(50)
                .HasColumnName("StudentID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<GetPlanCourseTerm>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetPlanCourseTerms");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.STerm)
                .HasMaxLength(20)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<GsbAuditLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("GSB_AuditLogEntries_pkey");

            entity.ToTable("GSB_AuditLogEntries");

            entity.HasIndex(e => e.PerformedByMakerUserId, "IX_GSB_AuditLogEntries_PerformedByMakerUserId");

            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.EntityId)
                .HasMaxLength(50)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.EntityName)
                .HasMaxLength(100)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.PerformedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.PerformedByMakerUser).WithMany(p => p.GsbAuditLogEntries)
                .HasForeignKey(d => d.PerformedByMakerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("GSB_AuditLogEntries_PerformedByMakerUserId_fkey");
        });

        modelBuilder.Entity<GsbMakerUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("GSB_MakerUsers_pkey");

            entity.ToTable("GSB_MakerUsers");

            entity.HasIndex(e => e.SchoolId, "IX_GSB_MakerUsers_SchoolId");

            entity.HasIndex(e => e.Username, "IX_GSB_MakerUsers_Username").IsUnique();

            entity.Property(e => e.ClassRoom)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.EncryptedPassword).HasDefaultValueSql("''::text");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Title)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<GsbReportBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("GSB_ReportBatches_pkey");

            entity.ToTable("GSB_ReportBatches");

            entity.HasIndex(e => e.BatchReference, "IX_GSB_ReportBatches_BatchReference").IsUnique();

            entity.HasIndex(e => e.GeneratedByMakerUserId, "IX_GSB_ReportBatches_GeneratedByMakerUserId");

            entity.HasIndex(e => e.SchoolId, "IX_GSB_ReportBatches_SchoolId");

            entity.Property(e => e.BatchReference).HasMaxLength(30);
            entity.Property(e => e.CoversDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GeneratedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Period)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.ReportType)
                .HasMaxLength(50)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.SavingsType)
                .HasMaxLength(50)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.StoragePath)
                .HasMaxLength(400)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.HasOne(d => d.GeneratedByMakerUser).WithMany(p => p.GsbReportBatches)
                .HasForeignKey(d => d.GeneratedByMakerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("GSB_ReportBatches_GeneratedByMakerUserId_fkey");
        });

        modelBuilder.Entity<GsbSavingsDeposit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("GSB_SavingsDeposits_pkey");

            entity.ToTable("GSB_SavingsDeposits");

            entity.HasIndex(e => e.IdempotencyKey, "IX_GSB_SavingsDeposits_IdempotencyKey").IsUnique();

            entity.HasIndex(e => e.RecordedByMakerUserId, "IX_GSB_SavingsDeposits_RecordedByMakerUserId");

            entity.HasIndex(e => e.Reference, "IX_GSB_SavingsDeposits_Reference").IsUnique();

            entity.HasIndex(e => e.ReportBatchId, "IX_GSB_SavingsDeposits_ReportBatchId");

            entity.HasIndex(e => e.SchoolId, "IX_GSB_SavingsDeposits_SchoolId");

            entity.HasIndex(e => e.StudentId, "IX_GSB_SavingsDeposits_StudentId");

            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100);
            entity.Property(e => e.OccurredAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Period)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Reference).HasMaxLength(30);
            entity.Property(e => e.SavingsType)
                .HasMaxLength(50)
                .HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.RecordedByMakerUser).WithMany(p => p.GsbSavingsDeposits)
                .HasForeignKey(d => d.RecordedByMakerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("GSB_SavingsDeposits_RecordedByMakerUserId_fkey");

            entity.HasOne(d => d.ReportBatch).WithMany(p => p.GsbSavingsDeposits)
                .HasForeignKey(d => d.ReportBatchId)
                .HasConstraintName("GSB_SavingsDeposits_ReportBatchId_fkey");
        });

        modelBuilder.Entity<Indexupdated>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("indexupdated");

            entity.Property(e => e.DuplicateCount).HasColumnName("duplicate_count");
        });

        modelBuilder.Entity<LessonPlan>(entity =>
        {
            entity.ToTable("Lesson_Plan");

            entity.HasIndex(e => new { e.SchoolId, e.EmpId, e.Status }, "IX_Lesson_Plan_School_Emp_Status").HasFilter("(\"CDel\" = false)");

            entity.HasIndex(e => new { e.SchoolId, e.SplaneId }, "IX_Lesson_Plan_School_Plane").HasFilter("(\"CDel\" = false)");

            entity.HasIndex(e => new { e.SchoolId, e.Status, e.Nyear, e.Nterm }, "IX_Lesson_Plan_School_Status_Term").HasFilter("(\"CDel\" = false)");

            entity.HasIndex(e => new { e.SchoolId, e.EmpId, e.Nyear, e.Nterm, e.SplaneId, e.NtsubLevel }, "UX_Lesson_Plan_Identity_Year")
                .IsUnique()
                .HasFilter("(\"CDel\" = false)");

            entity.Property(e => e.LessonPlanId).HasColumnName("LessonPlanID");
            entity.Property(e => e.Cdel).HasColumnName("CDel");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.EmpId).HasColumnName("EmpID");
            entity.Property(e => e.LatestSubmissionNo).HasDefaultValue(1);
            entity.Property(e => e.Nterm)
                .HasMaxLength(20)
                .HasColumnName("NTerm");
            entity.Property(e => e.NtsubLevel).HasColumnName("NTSubLevel");
            entity.Property(e => e.Nyear).HasColumnName("NYear");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SplaneId).HasColumnName("SPlaneID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying");
        });

        modelBuilder.Entity<LessonPlanFile>(entity =>
        {
            entity.ToTable("Lesson_Plan_File");

            entity.HasIndex(e => e.ObjectKey, "IX_Lesson_Plan_File_Object_Key");

            entity.HasIndex(e => e.LessonPlanSubmissionId, "IX_Lesson_Plan_File_Submission_All");

            entity.HasIndex(e => new { e.SchoolId, e.CreatedAt }, "IX_Lesson_Plan_File_Unattached").HasFilter("((\"LessonPlanSubmissionID\" IS NULL) AND (\"CDel\" = false))");

            entity.Property(e => e.LessonPlanFileId).HasColumnName("LessonPlanFileID");
            entity.Property(e => e.Cdel).HasColumnName("CDel");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.LessonPlanSubmissionId).HasColumnName("LessonPlanSubmissionID");
            entity.Property(e => e.MimeType).HasMaxLength(150);
            entity.Property(e => e.ObjectKey).HasMaxLength(512);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

            entity.HasOne(d => d.LessonPlanSubmission).WithMany(p => p.LessonPlanFiles)
                .HasForeignKey(d => d.LessonPlanSubmissionId)
                .HasConstraintName("FK_Lesson_Plan_File_Lesson_Plan_Submission");
        });

        modelBuilder.Entity<LessonPlanSubmission>(entity =>
        {
            entity.ToTable("Lesson_Plan_Submission");

            entity.HasIndex(e => e.LessonPlanId, "IX_Lesson_Plan_Submission_Plan_All");

            entity.HasIndex(e => new { e.LessonPlanId, e.Status }, "IX_Lesson_Plan_Submission_Plan_Status").HasFilter("(\"CDel\" = false)");

            entity.HasIndex(e => new { e.SchoolId, e.SubmittedAt }, "IX_Lesson_Plan_Submission_School_Submitted")
                .IsDescending(false, true)
                .HasFilter("(\"CDel\" = false)");

            entity.HasIndex(e => new { e.LessonPlanId, e.SubmissionNo }, "UX_Lesson_Plan_Submission_Plan_No")
                .IsUnique()
                .HasFilter("(\"CDel\" = false)");

            entity.Property(e => e.LessonPlanSubmissionId).HasColumnName("LessonPlanSubmissionID");
            entity.Property(e => e.Cdel).HasColumnName("CDel");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.LessonPlanId).HasColumnName("LessonPlanID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying");
            entity.Property(e => e.SubmissionNo).HasColumnName("Submission_No");
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.LessonPlan).WithMany(p => p.LessonPlanSubmissions)
                .HasForeignKey(d => d.LessonPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lesson_Plan_Submission_Lesson_Plan");
        });

        modelBuilder.Entity<MasterDbtuser>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("MasterDBTUser");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasColumnType("character varying")
                .HasColumnName("cType");
            entity.Property(e => e.NCompany).HasColumnName("nCompany");
            entity.Property(e => e.NMoney).HasColumnName("nMoney");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NSystemId).HasColumnName("nSystemID");
            entity.Property(e => e.Nfc)
                .HasColumnType("character varying")
                .HasColumnName("NFC");
            entity.Property(e => e.PasswordHash).HasColumnType("character varying");
            entity.Property(e => e.Pin)
                .HasColumnType("character varying")
                .HasColumnName("pin");
            entity.Property(e => e.SEmail)
                .HasColumnType("character varying")
                .HasColumnName("sEmail");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SIdentification)
                .HasColumnType("character varying")
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasColumnType("character varying")
                .HasColumnName("sName");
            entity.Property(e => e.SPicture)
                .HasColumnType("character varying")
                .HasColumnName("sPicture");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(200)
                .HasColumnName("sStudentID");
            entity.Property(e => e.Username)
                .HasColumnType("character varying")
                .HasColumnName("username");
            entity.Property(e => e.Userpassword)
                .HasColumnType("character varying")
                .HasColumnName("userpassword");
        });

        modelBuilder.Entity<MasterDbtuserCard>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.SId, e.SchoolId }).HasName("public_MasterDBTUserCard_PK_MasterDBTUserCard");

            entity.ToTable("MasterDBTUserCard");

            entity.HasIndex(e => new { e.SId, e.SchoolId }, "idx_masterdbtusercard_active").HasFilter("(COALESCE(\"IsActive\", true) AND (NOT COALESCE(\"IsDel\", false)))");

            entity.HasIndex(e => new { e.SId, e.SchoolId }, "idx_masterdbtusercard_sid_school_active").HasFilter("((COALESCE(\"IsActive\", true) = true) AND (NOT COALESCE(\"IsDel\", false)))");

            entity.HasIndex(e => e.Nfc, "idx_tusercard_nfc")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.Nfcencrypt, "idx_tusercard_nfcencrypt")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.NfcencryptReverse, "idx_tusercard_nfcencryptreverse")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.Nfcreverse, "idx_tusercard_nfcreverse")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.No, "idx_tusercard_no")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.IsActive, e.IsDel }, "idx_tusercard_school_active")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.SId, e.IsDel }, "idx_tusercard_school_sid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SId, e.SchoolId }, "idx_tusercard_sid_school")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FreeText).HasMaxLength(20);
            entity.Property(e => e.Modified).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Nfc)
                .HasMaxLength(20)
                .HasColumnName("NFC");
            entity.Property(e => e.Nfcencrypt)
                .HasMaxLength(20)
                .HasColumnName("NFCEncrypt");
            entity.Property(e => e.NfcencryptReverse)
                .HasMaxLength(20)
                .HasColumnName("NFCEncryptReverse");
            entity.Property(e => e.Nfcreverse)
                .HasMaxLength(20)
                .HasColumnName("NFCReverse");
        });

        modelBuilder.Entity<MvSchoolList>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("MV_SchoolList");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("province");

            entity.Property(e => e.GeoId).HasColumnName("GEO_ID");
            entity.Property(e => e.ProvinceCode)
                .HasMaxLength(2)
                .HasColumnName("PROVINCE_CODE");
            entity.Property(e => e.ProvinceId).HasColumnName("PROVINCE_ID");
            entity.Property(e => e.ProvinceName)
                .HasMaxLength(150)
                .HasColumnName("PROVINCE_NAME");
            entity.Property(e => e.ProvinceNameEn)
                .HasMaxLength(150)
                .HasColumnName("PROVINCE_NAME_EN");
        });

        modelBuilder.Entity<ResultVerifyUserBalanceBkup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ResultVerifyUserBalance_Bkup");

            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.BusinessDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CType)
                .HasMaxLength(5)
                .HasColumnName("cType");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Sqlstatement)
                .HasMaxLength(99)
                .HasColumnName("SQLStatement");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<ResultVerifyUserBalanceTempCard>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ResultVerifyUserBalanceTempCard");

            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.CardHistoryId).HasColumnName("CardHistoryID");
            entity.Property(e => e.Sqlstatement).HasColumnName("SQLStatement");
        });

        modelBuilder.Entity<ResultVerifyUserBalanceTempCardBkup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ResultVerifyUserBalanceTempCard_Bkup");

            entity.Property(e => e.Balance)
                .HasPrecision(18, 2)
                .HasColumnName("balance");
            entity.Property(e => e.BusinessDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CardHistoryId).HasColumnName("CardHistoryID");
            entity.Property(e => e.Sqlstatement)
                .HasMaxLength(560)
                .HasColumnName("SQLStatement");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<SbloginHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_SBLoginHistory_PK_SBLoginHistory");

            entity.ToTable("SBLoginHistory");

            entity.HasIndex(e => new { e.SchoolId, e.NStudentId, e.LoginAt }, "public_SBLoginHistory_IDX_SBLoginHistory_Covering");

            entity.HasIndex(e => e.SchoolId, "public_SBLoginHistory_INDEX_VV_126133_126132");

            entity.HasIndex(e => e.SImei, "public_SBLoginHistory_INDEX_VV_126711_126710");

            entity.HasIndex(e => new { e.SchoolId, e.NStudentId, e.LoginAt }, "public_SBLoginHistory_IX_SBLoginHistory_School_Student_LoginAt");

            entity.HasIndex(e => new { e.SchoolId, e.NStudentId, e.LoginAt }, "public_SBLoginHistory_IX_SBLoginHistory_School_Student_LoginAt_");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LoginAt)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("loginAt");
            entity.Property(e => e.NStudentId).HasColumnName("nStudentID");
            entity.Property(e => e.SAppName)
                .HasMaxLength(100)
                .HasColumnName("sAppName");
            entity.Property(e => e.SAppVersion)
                .HasMaxLength(100)
                .HasColumnName("sAppVersion");
            entity.Property(e => e.SDeviceOs)
                .HasMaxLength(450)
                .HasColumnName("sDeviceOS");
            entity.Property(e => e.SFirstName)
                .HasMaxLength(100)
                .HasColumnName("sFirstName");
            entity.Property(e => e.SImei)
                .HasMaxLength(450)
                .HasColumnName("sIMEI");
            entity.Property(e => e.SIpaddress)
                .HasMaxLength(150)
                .HasColumnName("sIPAddress");
            entity.Property(e => e.SLastName)
                .HasMaxLength(100)
                .HasColumnName("sLastName");
            entity.Property(e => e.SchoolId).HasColumnName("schoolID");
        });

        modelBuilder.Entity<SchoolDbtemployee>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("SchoolDBTEmployee");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(10)
                .HasColumnName("cType");
            entity.Property(e => e.NMax).HasColumnName("nMax");
            entity.Property(e => e.NMoney).HasColumnName("nMoney");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SPicture)
                .HasMaxLength(250)
                .HasColumnName("sPicture");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<SchoolDbtuser>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("SchoolDBTUser");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.NMax).HasColumnName("nMax");
            entity.Property(e => e.NMoney).HasColumnName("nMoney");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentPicture)
                .HasMaxLength(255)
                .HasColumnName("sStudentPicture");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<SchoolEmployeeSyncQueue>(entity =>
        {
            entity.HasKey(e => e.QueueId).HasName("SchoolEmployeeSyncQueue_pkey");

            entity.ToTable("SchoolEmployeeSyncQueue");

            entity.HasIndex(e => new { e.SchoolId, e.SEmp }, "idx_schoolemployeesyncqueue_school_sid_unprocessed")
                .HasFilter("(\"Processed\" = false)")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.Property(e => e.QueueId).HasColumnName("QueueID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Processed).HasDefaultValue(false);
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<SchoolSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("school_settings_pkey");

            entity.ToTable("school_settings");

            entity.HasIndex(e => new { e.SchoolId, e.SettingKey }, "ux_school_settings_school_id_setting_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .HasColumnName("setting_key");
            entity.Property(e => e.SettingValue)
                .HasMaxLength(255)
                .HasColumnName("setting_value");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(255)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<SchoolTuser>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("SchoolTUser");

            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<SchoolUserSyncQueue>(entity =>
        {
            entity.HasKey(e => e.QueueId).HasName("SchoolUserSyncQueue_pkey");

            entity.ToTable("SchoolUserSyncQueue");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "idx_schooluser_school_sid_unprocessed")
                .HasFilter("(\"Processed\" = false)")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.Property(e => e.QueueId).HasColumnName("QueueID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Processed).HasDefaultValue(false);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<SearchBySidtemp>(entity =>
        {
            entity.HasKey(e => e.SId);

            entity.ToTable("SearchBySIDTemp");

            entity.Property(e => e.SId)
                .ValueGeneratedNever()
                .HasColumnName("sID");
        });

        modelBuilder.Entity<SearchPlanCourseDetailTemp>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("SearchPlanCourseDetailTemp");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
        });

        modelBuilder.Entity<TFsdqAnswer>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TFsdqAnswerId }).HasName("public_T_FSDQ_Answer_PK_T_FSDQ_Answer");

            entity.ToTable("T_FSDQ_Answer");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TFsdqAnswerId)
                .ValueGeneratedOnAdd()
                .HasColumnName("T_FSDQ_Answer_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SId).HasColumnName("sId");
            entity.Property(e => e.TFsdqPointPoint).HasColumnName("T_FSDQ_Point_Point");
            entity.Property(e => e.TFsdqQuestionId).HasColumnName("T_FSDQ_Question_Id");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TFsdqDatum>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TFsdqDataId }).HasName("public_T_FSDQ_Data_PK_T_FSDQ_Data");

            entity.ToTable("T_FSDQ_Data");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TFsdqDataId)
                .ValueGeneratedOnAdd()
                .HasColumnName("T_FSDQ_Data_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.TFsdqDataDatetime)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("T_FSDQ_Data_Datetime");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TFsdqPoint>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TFsdqPointId }).HasName("public_T_FSDQ_Point_PK_T_FSDQ_Point");

            entity.ToTable("T_FSDQ_Point");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TFsdqPointId)
                .ValueGeneratedOnAdd()
                .HasColumnName("T_FSDQ_Point_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TFsdqPointPoint).HasColumnName("T_FSDQ_Point_Point");
            entity.Property(e => e.TFsdqQuestionId).HasColumnName("T_FSDQ_Question_Id");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TFsdqQuestion>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TFsdqQuestionId }).HasName("public_T_FSDQ_Question_PK_T_FSDQ_Question");

            entity.ToTable("T_FSDQ_Question");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TFsdqQuestionId)
                .ValueGeneratedOnAdd()
                .HasColumnName("T_FSDQ_Question_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TFsdqQuestionDes)
                .HasMaxLength(250)
                .HasColumnName("T_FSDQ_Question_Des");
            entity.Property(e => e.TFsdqQuestionGroup).HasColumnName("T_FSDQ_Question_Group");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TapproveEmployeeContactAddress>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeeContactAddress_PK_TAPPROVEEMPLOYEECONTAC");

            entity.ToTable("TApproveEmployeeContactAddress");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.Building).HasMaxLength(50);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HouseNo).HasMaxLength(10);
            entity.Property(e => e.Moo).HasMaxLength(10);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Road).HasMaxLength(40);
            entity.Property(e => e.Soi).HasMaxLength(40);
            entity.Property(e => e.Village).HasMaxLength(50);
        });

        modelBuilder.Entity<TapproveEmployeeEducation>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeeEducation_PK_TAPPROVEEMPLOYEEEDUCATION");

            entity.ToTable("TApproveEmployeeEducation");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.EducationInstitution).HasMaxLength(100);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Major).HasMaxLength(100);
            entity.Property(e => e.Minor).HasMaxLength(100);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TapproveEmployeeFamily>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeeFamily_PK_TAPPROVEEMPLOYEEFAMILY");

            entity.ToTable("TApproveEmployeeFamily");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.Birthday).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DeathStatus).HasMaxLength(30);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FamilyOccupation).HasMaxLength(50);
            entity.Property(e => e.FamilyRelationship).HasMaxLength(20);
            entity.Property(e => e.LivingStatus).HasMaxLength(30);
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Surname).HasMaxLength(50);
        });

        modelBuilder.Entity<TapproveEmployeeHistoryReceivingRoyalDecoration>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeeHistoryReceivingRoyalDecoration_PK_TAPPR");

            entity.ToTable("TApproveEmployeeHistoryReceivingRoyalDecoration");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.Dated).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.RoyalDecorationClass).HasMaxLength(50);
            entity.Property(e => e.RoyalGovernmentNo).HasMaxLength(10);
            entity.Property(e => e.RoyalGovernmentPart).HasMaxLength(10);
            entity.Property(e => e.RoyalGovernmentVolumeNo).HasMaxLength(10);
        });

        modelBuilder.Entity<TapproveEmployeeHonor>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeeHonor_PK_TAPPROVEEMPLOYEEHONOR");

            entity.ToTable("TApproveEmployeeHonor");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.DonatingAgency).HasMaxLength(100);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HonorType).HasMaxLength(100);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TapproveEmployeeOccupationalLicense>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeeOccupationalLicense_PK_TAPPROVEEMPLOYEEO");

            entity.ToTable("TApproveEmployeeOccupationalLicense");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.CertificateExpireDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CertificateIssueDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.OrganizationIssuingCertificate).HasMaxLength(100);
            entity.Property(e => e.ProfessionalLicenseName).HasMaxLength(100);
            entity.Property(e => e.ProfessionalLicenseNumber).HasMaxLength(20);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TapproveEmployeePermanentAddress>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeePermanentAddress_PK_TAPPROVEEMPLOYEEPERM");

            entity.ToTable("TApproveEmployeePermanentAddress");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.Building).HasMaxLength(50);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HouseNo).HasMaxLength(100);
            entity.Property(e => e.Moo).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Road).HasMaxLength(50);
            entity.Property(e => e.Soi).HasMaxLength(50);
            entity.Property(e => e.Village).HasMaxLength(50);
        });

        modelBuilder.Entity<TapproveEmployeeProfile>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeeProfile_PK_TAPPROVEEMPLOYEEPROFILE");

            entity.ToTable("TApproveEmployeeProfile");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.BirthDay).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.BloodType).HasMaxLength(5);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Gender).HasMaxLength(1);
            entity.Property(e => e.IdcardNumber)
                .HasMaxLength(13)
                .HasColumnName("IDCardNumber");
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NameEn).HasMaxLength(30);
            entity.Property(e => e.Nationality).HasMaxLength(3);
            entity.Property(e => e.PassportCountry).HasMaxLength(100);
            entity.Property(e => e.PassportNumber).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Race).HasMaxLength(3);
            entity.Property(e => e.Religion).HasMaxLength(3);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SpouseName).HasMaxLength(30);
            entity.Property(e => e.SpouseSurname).HasMaxLength(50);
            entity.Property(e => e.Surname).HasMaxLength(256);
            entity.Property(e => e.SurnameEn).HasMaxLength(50);
        });

        modelBuilder.Entity<TapproveEmployeeTraining>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EmployeeId, e.SchoolId }).HasName("public_TApproveEmployeeTraining_PK_TAPPROVEEMPLOYEETRAINING");

            entity.ToTable("TApproveEmployeeTraining");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Expenses).HasPrecision(6, 2);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.NumberHours).HasPrecision(6, 2);
            entity.Property(e => e.Project).HasMaxLength(100);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TrainingCourseName).HasMaxLength(100);
            entity.Property(e => e.TrainingEndDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TrainingStartDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TapproveStudentContactAddress>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.StudentId, e.SchoolId }).HasName("public_TApproveStudentContactAddress_PK_TAPPROVESTUDENTCONTACTA");

            entity.ToTable("TApproveStudentContactAddress");

            entity.HasIndex(e => e.ApproveStatus, "public_TApproveStudentContactAddress_INDEX_VV_1031_1030");

            entity.HasIndex(e => new { e.StudentId, e.SchoolId }, "public_TApproveStudentContactAddress_INDEX_VV_156524_156523");

            entity.HasIndex(e => e.SchoolId, "public_TApproveStudentContactAddress_INDEX_VV_22350_22349");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HouseNo).HasMaxLength(100);
            entity.Property(e => e.HousePhone).HasMaxLength(50);
            entity.Property(e => e.LiveWithEmail).HasMaxLength(100);
            entity.Property(e => e.LiveWithName).HasMaxLength(100);
            entity.Property(e => e.LiveWithSurname).HasMaxLength(100);
            entity.Property(e => e.Moo).HasMaxLength(50);
            entity.Property(e => e.NeighborName).HasMaxLength(50);
            entity.Property(e => e.NeighborPhone).HasMaxLength(30);
            entity.Property(e => e.NeighborSurname).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(50);
            entity.Property(e => e.RecoveryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.RecoveryScript).HasMaxLength(2000);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Road).HasMaxLength(50);
            entity.Property(e => e.Soi).HasMaxLength(50);
        });

        modelBuilder.Entity<TapproveStudentFatherInfo>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.StudentId, e.SchoolId }).HasName("public_TApproveStudentFatherInfo_PK_TAPPROVESTUDENTFATHERINFO");

            entity.ToTable("TApproveStudentFatherInfo");

            entity.HasIndex(e => new { e.StudentId, e.SchoolId }, "public_TApproveStudentFatherInfo_INDEX_VV_18210_18209");

            entity.HasIndex(e => e.SchoolId, "public_TApproveStudentFatherInfo_INDEX_VV_22352_22351");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.BirthDay).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Career).HasMaxLength(100);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HouseNo).HasMaxLength(50);
            entity.Property(e => e.IdcardNumber)
                .HasMaxLength(50)
                .HasColumnName("IDCardNumber");
            entity.Property(e => e.Moo).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NameEn).HasMaxLength(100);
            entity.Property(e => e.Nationality).HasMaxLength(3);
            entity.Property(e => e.PhoneNumberHouse).HasMaxLength(50);
            entity.Property(e => e.PhoneNumberMobile).HasMaxLength(20);
            entity.Property(e => e.PhoneNumberWorkPlace).HasMaxLength(20);
            entity.Property(e => e.PostalCode).HasMaxLength(50);
            entity.Property(e => e.Race).HasMaxLength(3);
            entity.Property(e => e.RecoveryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.RecoveryScript).HasMaxLength(2000);
            entity.Property(e => e.Religion).HasMaxLength(3);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Road).HasMaxLength(50);
            entity.Property(e => e.Soi).HasMaxLength(50);
            entity.Property(e => e.Surname).HasMaxLength(50);
            entity.Property(e => e.SurnameEn).HasMaxLength(100);
            entity.Property(e => e.WorkPlaces).HasMaxLength(200);
        });

        modelBuilder.Entity<TapproveStudentMotherInfo>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.StudentId, e.SchoolId }).HasName("public_TApproveStudentMotherInfo_PK_TAPPROVESTUDENTMOTHERINFO");

            entity.ToTable("TApproveStudentMotherInfo");

            entity.HasIndex(e => new { e.StudentId, e.SchoolId }, "public_TApproveStudentMotherInfo_INDEX_VV_156854_156853");

            entity.HasIndex(e => e.SchoolId, "public_TApproveStudentMotherInfo_INDEX_VV_22354_22353");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.BirthDay).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Career).HasMaxLength(100);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HouseNo).HasMaxLength(50);
            entity.Property(e => e.IdcardNumber)
                .HasMaxLength(50)
                .HasColumnName("IDCardNumber");
            entity.Property(e => e.Moo).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NameEn).HasMaxLength(100);
            entity.Property(e => e.Nationality).HasMaxLength(3);
            entity.Property(e => e.PhoneNumberHouse).HasMaxLength(50);
            entity.Property(e => e.PhoneNumberMobile).HasMaxLength(20);
            entity.Property(e => e.PhoneNumberWorkPlace).HasMaxLength(20);
            entity.Property(e => e.PostalCode).HasMaxLength(50);
            entity.Property(e => e.Race).HasMaxLength(3);
            entity.Property(e => e.RecoveryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.RecoveryScript).HasMaxLength(2000);
            entity.Property(e => e.Religion).HasMaxLength(3);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Road).HasMaxLength(50);
            entity.Property(e => e.Soi).HasMaxLength(50);
            entity.Property(e => e.Surname).HasMaxLength(50);
            entity.Property(e => e.SurnameEn).HasMaxLength(100);
            entity.Property(e => e.WorkPlaces).HasMaxLength(200);
        });

        modelBuilder.Entity<TapproveStudentParentInfo>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.StudentId, e.SchoolId }).HasName("public_TApproveStudentParentInfo_PK_TAPPROVESTUDENTPARENTINFO");

            entity.ToTable("TApproveStudentParentInfo");

            entity.HasIndex(e => new { e.StudentId, e.SchoolId }, "public_TApproveStudentParentInfo_INDEX_VV_157518_157517");

            entity.HasIndex(e => e.SchoolId, "public_TApproveStudentParentInfo_INDEX_VV_22356_22355");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.BirthDay).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Career).HasMaxLength(200);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HouseNo).HasMaxLength(50);
            entity.Property(e => e.IdcardNumber)
                .HasMaxLength(20)
                .HasColumnName("IDCardNumber");
            entity.Property(e => e.Moo).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NameEn).HasMaxLength(50);
            entity.Property(e => e.Nationality).HasMaxLength(3);
            entity.Property(e => e.PhoneNumberHouse).HasMaxLength(30);
            entity.Property(e => e.PhoneNumberMobile).HasMaxLength(30);
            entity.Property(e => e.PhoneNumberWorkPlace).HasMaxLength(30);
            entity.Property(e => e.PostalCode).HasMaxLength(50);
            entity.Property(e => e.Race).HasMaxLength(3);
            entity.Property(e => e.RecoveryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.RecoveryScript).HasMaxLength(2000);
            entity.Property(e => e.Relationship).HasMaxLength(20);
            entity.Property(e => e.Religion).HasMaxLength(3);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Road).HasMaxLength(50);
            entity.Property(e => e.Soi).HasMaxLength(50);
            entity.Property(e => e.Surname).HasMaxLength(50);
            entity.Property(e => e.SurnameEn).HasMaxLength(50);
            entity.Property(e => e.WorkPlaces).HasMaxLength(200);
        });

        modelBuilder.Entity<TapproveStudentPermanentAddress>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.StudentId, e.SchoolId }).HasName("public_TApproveStudentPermanentAddress_PK_TAPPROVESTUDENTPERMAN");

            entity.ToTable("TApproveStudentPermanentAddress");

            entity.HasIndex(e => new { e.StudentId, e.SchoolId }, "public_TApproveStudentPermanentAddress_INDEX_VV_145218_145217");

            entity.HasIndex(e => e.SchoolId, "public_TApproveStudentPermanentAddress_INDEX_VV_22348_22347");

            entity.HasIndex(e => new { e.SchoolId, e.ApproveStatus }, "public_TApproveStudentPermanentAddress_INDEX_VV_23788_23787");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.BirthPlace).HasMaxLength(100);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HomePhoneNumber).HasMaxLength(20);
            entity.Property(e => e.HouseCode).HasMaxLength(20);
            entity.Property(e => e.HouseNo).HasMaxLength(100);
            entity.Property(e => e.Moo).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.RecoveryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.RecoveryScript).HasMaxLength(2000);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Road).HasMaxLength(100);
            entity.Property(e => e.Soi).HasMaxLength(100);
        });

        modelBuilder.Entity<TapproveStudentProfile>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.StudentId, e.SchoolId }).HasName("public_TApproveStudentProfile_PK_TAPPROVESTUDENTPROFILE");

            entity.ToTable("TApproveStudentProfile");

            entity.HasIndex(e => e.SchoolId, "public_TApproveStudentProfile_INDEX_VV_22346_22345");

            entity.HasIndex(e => e.SchoolId, "public_TApproveStudentProfile_INDEX_VV_23527_23526");

            entity.HasIndex(e => new { e.StudentId, e.SchoolId }, "public_TApproveStudentProfile_INDEX_VV_5794232_5794231");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ApproveComment).HasMaxLength(500);
            entity.Property(e => e.ApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ApproveStatus).HasMaxLength(20);
            entity.Property(e => e.BirthDay).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Disability).HasMaxLength(2);
            entity.Property(e => e.Disadvantaged).HasMaxLength(2);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.EndDateShowResult).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Gender).HasMaxLength(1);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NameEn).HasMaxLength(50);
            entity.Property(e => e.NameOther).HasMaxLength(50);
            entity.Property(e => e.Nationality).HasMaxLength(3);
            entity.Property(e => e.Nickname).HasMaxLength(256);
            entity.Property(e => e.NicknameEn).HasMaxLength(256);
            entity.Property(e => e.Other).HasMaxLength(1000);
            entity.Property(e => e.PhoneNumber).HasMaxLength(100);
            entity.Property(e => e.Race).HasMaxLength(3);
            entity.Property(e => e.RecoveryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.RecoveryScript).HasMaxLength(2000);
            entity.Property(e => e.Religion).HasMaxLength(3);
            entity.Property(e => e.RequestApproveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SpendingPerDay).HasPrecision(18, 2);
            entity.Property(e => e.Surname).HasMaxLength(256);
            entity.Property(e => e.SurnameEn).HasMaxLength(50);
            entity.Property(e => e.SurnameOther).HasMaxLength(50);
        });

        modelBuilder.Entity<TbCurrentTermDatum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_CurrentTermData");

            entity.Property(e => e.DEnd).HasColumnName("dEnd");
            entity.Property(e => e.DStart).HasColumnName("dStart");
            entity.Property(e => e.NTerm)
                .HasColumnType("character varying")
                .HasColumnName("nTerm");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.STerm).HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TbEqAnswer>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TbEqAnswerId }).HasName("public_TB_EQ_Answer_PK_TB_EQ_Answer");

            entity.ToTable("TB_EQ_Answer");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TbEqAnswerId)
                .ValueGeneratedOnAdd()
                .HasColumnName("TB_EQ_Answer_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.TbEqPointPoint).HasColumnName("TB_EQ_Point_Point");
            entity.Property(e => e.TbEqQuestionId).HasColumnName("TB_EQ_Question_Id");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbEqDatum>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TbEqDataId }).HasName("public_TB_EQ_Data_PK_TB_EQ_Data");

            entity.ToTable("TB_EQ_Data");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TbEqDataId)
                .ValueGeneratedOnAdd()
                .HasColumnName("TB_EQ_Data_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.TbEqDataDatetime)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("TB_EQ_Data_Datetime");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbEqGroup>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TbEqGroupId }).HasName("public_TB_EQ_Group_PK_TB_EQ_Group");

            entity.ToTable("TB_EQ_Group");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TbEqGroupId)
                .ValueGeneratedOnAdd()
                .HasColumnName("TB_EQ_Group_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TbEqGroupDes)
                .HasMaxLength(250)
                .HasColumnName("TB_EQ_Group_Des");
            entity.Property(e => e.TbEqQuestionGroup).HasColumnName("TB_EQ_Question_Group");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbEqPoint>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TbEqPointId }).HasName("public_TB_EQ_Point_PK_TB_EQ_Point");

            entity.ToTable("TB_EQ_Point");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TbEqPointId)
                .ValueGeneratedOnAdd()
                .HasColumnName("TB_EQ_Point_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TbEqPointPoint).HasColumnName("TB_EQ_Point_Point");
            entity.Property(e => e.TbEqQuestionId).HasColumnName("TB_EQ_Question_Id");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbEqQuestion>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TbEqQuestionId }).HasName("public_TB_EQ_Question_PK_TB_EQ_Question");

            entity.ToTable("TB_EQ_Question");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TbEqQuestionId)
                .ValueGeneratedOnAdd()
                .HasColumnName("TB_EQ_Question_Id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TbEqQuestionDes)
                .HasMaxLength(250)
                .HasColumnName("TB_EQ_Question_Des");
            entity.Property(e => e.TbEqQuestionGroup).HasColumnName("TB_EQ_Question_Group");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbHistorySetting>(entity =>
        {
            entity.HasKey(e => e.FdHistoryId).HasName("public_TB_HistorySetting_PK_TB_HistorySetting");

            entity.ToTable("TB_HistorySetting");

            entity.Property(e => e.FdHistoryId)
                .ValueGeneratedNever()
                .HasColumnName("Fd_HistoryID");
            entity.Property(e => e.FdFunctionName)
                .HasMaxLength(200)
                .HasColumnName("Fd_FunctionName");
            entity.Property(e => e.FdSchoolId).HasColumnName("Fd_SchoolID");
            entity.Property(e => e.FdSettingData).HasColumnName("Fd_SettingData");
            entity.Property(e => e.FdUpdatedBy).HasColumnName("Fd_UpdatedBy");
            entity.Property(e => e.FdUpdatedDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("Fd_UpdatedDate");
        });

        modelBuilder.Entity<TbSbloginHistoryFiltered>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_SBLoginHistoryFiltered");

            entity.Property(e => e.Rnk).HasColumnName("rnk");
            entity.Property(e => e.SImei)
                .HasMaxLength(450)
                .HasColumnName("sIMEI");
        });

        modelBuilder.Entity<TbStudentDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentDetails");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
        });

        modelBuilder.Entity<TbStudentView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ClassFullName).HasMaxLength(50);
            entity.Property(e => e.ClassFullNameEng).HasMaxLength(50);
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(256)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentTypeId).HasColumnName("StudentTypeID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.SubLevelEn)
                .HasMaxLength(10)
                .HasColumnName("SubLevelEN");
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
            entity.Property(e => e.TitleDescriptionEn)
                .HasColumnType("character varying")
                .HasColumnName("titleDescriptionEn");
        });

        modelBuilder.Entity<TbStudentViewsAcademic>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Academic");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ClassFullName).HasMaxLength(50);
            entity.Property(e => e.ClassFullNameEng).HasMaxLength(50);
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.Note).HasMaxLength(250);
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(256)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentTypeId).HasColumnName("StudentTypeID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.SubLevelEn)
                .HasMaxLength(10)
                .HasColumnName("SubLevelEN");
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
            entity.Property(e => e.TitleDescriptionEn)
                .HasColumnType("character varying")
                .HasColumnName("titleDescriptionEn");
        });

        modelBuilder.Entity<TbStudentViewsAcademicOpt>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Academic_Opt");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ClassFullName).HasMaxLength(50);
            entity.Property(e => e.ClassFullNameEng).HasMaxLength(50);
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.Efkey).HasColumnName("EFKey");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(256)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentTypeId).HasColumnName("StudentTypeID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.SubLevelEn)
                .HasMaxLength(10)
                .HasColumnName("SubLevelEN");
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
            entity.Property(e => e.TitleDescriptionEn)
                .HasColumnType("character varying")
                .HasColumnName("titleDescriptionEn");
        });

        modelBuilder.Entity<TbStudentViewsCurrentTerm>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_CurrentTerm");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ClassFullName).HasMaxLength(50);
            entity.Property(e => e.ClassFullNameEng).HasMaxLength(50);
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd).HasColumnName("dEnd");
            entity.Property(e => e.DStart).HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(256)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm).HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentTypeId).HasColumnName("StudentTypeID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.SubLevelEn)
                .HasMaxLength(10)
                .HasColumnName("SubLevelEN");
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
            entity.Property(e => e.TitleDescriptionEn)
                .HasColumnType("character varying")
                .HasColumnName("titleDescriptionEn");
        });

        modelBuilder.Entity<TbStudentViewsCurrentTermLite>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_CurrentTerm_Lite");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TbStudentViewsCurrentTermLite1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_CurrentTerm_Lite_1");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TbStudentViewsCurrentTermLite2>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_CurrentTerm_Lite_2");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TbStudentViewsCurrentTermLiteOrig>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_CurrentTerm_Lite_Orig");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TbStudentViewsFiltered>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Filtered");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ClassFullName).HasMaxLength(50);
            entity.Property(e => e.ClassFullNameEng).HasMaxLength(50);
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
        });

        modelBuilder.Entity<TbStudentViewsMemory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Memory");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TbStudentViewsMemoryOpt>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Memory_Opt");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TbStudentViewsMemoryOpt1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Memory_Opt_1");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TbStudentViewsOptimized>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Optimized");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ClassFullName).HasMaxLength(50);
            entity.Property(e => e.ClassFullNameEng).HasMaxLength(50);
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(256)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentTypeId).HasColumnName("StudentTypeID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.SubLevelEn)
                .HasMaxLength(10)
                .HasColumnName("SubLevelEN");
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
            entity.Property(e => e.TitleDescriptionEn)
                .HasColumnType("character varying")
                .HasColumnName("titleDescriptionEn");
        });

        modelBuilder.Entity<TbStudentViewsPicture>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Picture");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ClassFullName).HasMaxLength(50);
            entity.Property(e => e.ClassFullNameEng).HasMaxLength(50);
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentPicture)
                .HasMaxLength(255)
                .HasColumnName("sStudentPicture");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.SubLevelEn)
                .HasMaxLength(10)
                .HasColumnName("SubLevelEN");
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
            entity.Property(e => e.TitleDescriptionEn)
                .HasColumnType("character varying")
                .HasColumnName("titleDescriptionEn");
        });

        modelBuilder.Entity<TbStudentViewsTest>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViews_Test");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.ClassFullName).HasMaxLength(50);
            entity.Property(e => e.ClassFullNameEng).HasMaxLength(50);
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(256)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentTypeId).HasColumnName("StudentTypeID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.SubLevelEn)
                .HasMaxLength(10)
                .HasColumnName("SubLevelEN");
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
            entity.Property(e => e.TitleDescriptionEn)
                .HasColumnType("character varying")
                .HasColumnName("titleDescriptionEn");
        });

        modelBuilder.Entity<TbStudentViewsV2>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViewsV2");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CSex)
                .HasMaxLength(1)
                .HasColumnName("cSex");
            entity.Property(e => e.DBirth)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dBirth");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
        });

        modelBuilder.Entity<TbStudentViewsV3>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TB_StudentViewsV3");

            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CSex)
                .HasMaxLength(1)
                .HasColumnName("cSex");
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.NDeleted).HasColumnName("nDeleted");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTermSubLevel2Status)
                .HasMaxLength(1)
                .HasColumnName("nTermSubLevel2Status");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NWorkingStatus).HasColumnName("nWorkingStatus");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentPicture)
                .HasMaxLength(255)
                .HasColumnName("sStudentPicture");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.TitleDescription)
                .HasColumnType("character varying")
                .HasColumnName("titleDescription");
        });

        modelBuilder.Entity<TbackupCard>(entity =>
        {
            entity.HasKey(e => new { e.CardId, e.SchoolId }).HasName("public_TBackupCard_PK_TBackupCard_1");

            entity.ToTable("TBackupCard");

            entity.HasIndex(e => new { e.BarCode, e.CDel }, "idx_tbackupcard_barcode_cdel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CardId, e.SchoolId }, "idx_tbackupcard_cardid_schoolid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.CardName, "idx_tbackupcard_cardname")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.Money, "idx_tbackupcard_money")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.Nfc, e.CDel }, "idx_tbackupcard_nfc_cdel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.SchoolId, "idx_tbackupcard_schoolid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.BarCode, e.CDel }, "idx_tbackupcard_schoolid_barcode_cdel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CardId, e.SchoolId, e.CDel }, "idx_tbackupcard_schoolid_cardid");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "idx_tbackupcard_schoolid_cdel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.NfcencryptReverse }, "idx_tbackupcard_schoolid_cdel_nfcencryptreverse")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.Money }, "idx_tbackupcard_schoolid_money")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.BarCode).HasMaxLength(20);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CardName).HasMaxLength(250);
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Insurance).HasPrecision(6, 2);
            entity.Property(e => e.Money).HasPrecision(18, 2);
            entity.Property(e => e.Nfc)
                .HasMaxLength(20)
                .HasColumnName("NFC");
            entity.Property(e => e.Nfcencrypt)
                .HasMaxLength(20)
                .HasColumnName("NFCEncrypt");
            entity.Property(e => e.NfcencryptReverse)
                .HasMaxLength(20)
                .HasColumnName("NFCEncryptReverse");
            entity.Property(e => e.Nfcreverse)
                .HasMaxLength(20)
                .HasColumnName("NFCReverse");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbackupCardHistory>(entity =>
        {
            entity.HasKey(e => new { e.CardHistoryId, e.SchoolId }).HasName("public_TBackupCardHistory_PK_TBackupCardHistory");

            entity.ToTable("TBackupCardHistory");

            entity.HasIndex(e => e.CardId, "idx_tbackupcardhistory_cardid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CardId, e.SchoolId, e.ReturnDate, e.CDel }, "idx_tbackupcardhistory_cardid_schoolid");

            entity.HasIndex(e => new { e.ReturnDate, e.CardId }, "idx_tbackupcardhistory_returndate_cardid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.ReturnDate, e.CDel }, "idx_tbackupcardhistory_returndate_cdel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.SchoolId, "idx_tbackupcardhistory_schoolid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.CardId }, "idx_tbackupcardhistory_schoolid_cardid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "idx_tbackupcardhistory_schoolid_cdel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.ReturnDate, e.BorrowingDate, e.CDel }, "idx_tbackupcardhistory_schoolid_return_borrow_cdel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.ReturnDate }, "idx_tbackupcardhistory_schoolid_returndate")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.UserId, "idx_tbackupcardhistory_userid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.UserType, "idx_tbackupcardhistory_usertype")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.Property(e => e.CardHistoryId).HasColumnName("CardHistoryID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.BorrowingDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Insurance).HasPrecision(6, 2);
            entity.Property(e => e.ReturnDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.UserName).HasMaxLength(250);
        });

        modelBuilder.Entity<TblMasterAccidentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_accident_type_pkey");

            entity.ToTable("tbl_master_accident_type");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_accident_type_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterAllergy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_allergy_pkey");

            entity.ToTable("tbl_master_allergy");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_allergy_school");

            entity.HasIndex(e => e.AllergyTypeId, "ix_tbl_master_allergy_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AllergyTypeId).HasColumnName("allergy_type_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.AllergyType).WithMany(p => p.TblMasterAllergies)
                .HasForeignKey(d => d.AllergyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_master_allergy_allergy_type_id_fkey");
        });

        modelBuilder.Entity<TblMasterAllergyType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_allergy_type_pkey");

            entity.ToTable("tbl_master_allergy_type");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_allergy_type_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterDisease>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_disease_pkey");

            entity.ToTable("tbl_master_disease");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_disease_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterMedicine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_medicine_pkey");

            entity.ToTable("tbl_master_medicine");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_medicine_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterPriorityLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_priority_level_pkey");

            entity.ToTable("tbl_master_priority_level");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_priority_level_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.ColorHex)
                .HasMaxLength(20)
                .HasColumnName("color_hex");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.LevelGroup)
                .HasMaxLength(50)
                .HasColumnName("level_group");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_status_pkey");

            entity.ToTable("tbl_master_status");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_status_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.ColorHex)
                .HasMaxLength(20)
                .HasColumnName("color_hex");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.StatusGroup)
                .HasMaxLength(50)
                .HasColumnName("status_group");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterSymptom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_symptom_pkey");

            entity.ToTable("tbl_master_symptom");

            entity.HasIndex(e => e.SeverityLevelId, "ix_fk_11488892_tbl_master_symptom");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_symptom_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.SeverityLevelId).HasColumnName("severity_level_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.SeverityLevel).WithMany(p => p.TblMasterSymptoms)
                .HasForeignKey(d => d.SeverityLevelId)
                .HasConstraintName("tbl_master_symptom_severity_level_id_fkey");
        });

        modelBuilder.Entity<TblMasterTreatment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_treatment_pkey");

            entity.ToTable("tbl_master_treatment");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_treatment_school");

            entity.HasIndex(e => e.TreatmentTypeId, "ix_tbl_master_treatment_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.TreatmentTypeId).HasColumnName("treatment_type_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.TreatmentType).WithMany(p => p.TblMasterTreatments)
                .HasForeignKey(d => d.TreatmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_master_treatment_treatment_type_id_fkey");
        });

        modelBuilder.Entity<TblMasterTreatmentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_treatment_type_pkey");

            entity.ToTable("tbl_master_treatment_type");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_treatment_type_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_unit_pkey");

            entity.ToTable("tbl_master_unit");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_unit_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterVaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_vaccine_pkey");

            entity.ToTable("tbl_master_vaccine");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_vaccine_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMasterVisitType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_master_visit_type_pkey");

            entity.ToTable("tbl_master_visit_type");

            entity.HasIndex(e => e.SchoolId, "ix_tbl_master_visit_type_school");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameI18n)
                .HasColumnType("jsonb")
                .HasColumnName("name_i18n");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TblMedicalProfileAccident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_profile_accident_pkey");

            entity.ToTable("tbl_medical_profile_accident");

            entity.HasIndex(e => e.AccidentTypeId, "ix_fk_11489914_tbl_medical_profile_accid");

            entity.HasIndex(e => e.SeverityLevelId, "ix_fk_11489919_tbl_medical_profile_accid");

            entity.HasIndex(e => new { e.SchoolId, e.UserId }, "ix_tbl_medical_profile_accident_school_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccidentTypeId).HasColumnName("accident_type_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeverityLevelId).HasColumnName("severity_level_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.AccidentType).WithMany(p => p.TblMedicalProfileAccidents)
                .HasForeignKey(d => d.AccidentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_profile_accident_accident_type_id_fkey");

            entity.HasOne(d => d.SeverityLevel).WithMany(p => p.TblMedicalProfileAccidents)
                .HasForeignKey(d => d.SeverityLevelId)
                .HasConstraintName("tbl_medical_profile_accident_severity_level_id_fkey");
        });

        modelBuilder.Entity<TblMedicalProfileAllergy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_profile_allergy_pkey");

            entity.ToTable("tbl_medical_profile_allergy");

            entity.HasIndex(e => e.AllergyId, "ix_fk_11489883_tbl_medical_profile_aller");

            entity.HasIndex(e => e.SeverityLevelId, "ix_fk_11489888_tbl_medical_profile_aller");

            entity.HasIndex(e => new { e.SchoolId, e.UserId }, "ix_tbl_medical_profile_allergy_school_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AllergyId).HasColumnName("allergy_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeverityLevelId).HasColumnName("severity_level_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Allergy).WithMany(p => p.TblMedicalProfileAllergies)
                .HasForeignKey(d => d.AllergyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_profile_allergy_allergy_id_fkey");

            entity.HasOne(d => d.SeverityLevel).WithMany(p => p.TblMedicalProfileAllergies)
                .HasForeignKey(d => d.SeverityLevelId)
                .HasConstraintName("tbl_medical_profile_allergy_severity_level_id_fkey");
        });

        modelBuilder.Entity<TblMedicalProfileChronicDisease>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_profile_chronic_disease_pkey");

            entity.ToTable("tbl_medical_profile_chronic_disease");

            entity.HasIndex(e => e.DiseaseId, "ix_fk_11489865_tbl_medical_profile_chron");

            entity.HasIndex(e => e.SeverityLevelId, "ix_fk_11489870_tbl_medical_profile_chron");

            entity.HasIndex(e => new { e.SchoolId, e.UserId }, "ix_tbl_medical_profile_chronic_disease_school_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.DiseaseId).HasColumnName("disease_id");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeverityLevelId).HasColumnName("severity_level_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Disease).WithMany(p => p.TblMedicalProfileChronicDiseases)
                .HasForeignKey(d => d.DiseaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_profile_chronic_disease_disease_id_fkey");

            entity.HasOne(d => d.SeverityLevel).WithMany(p => p.TblMedicalProfileChronicDiseases)
                .HasForeignKey(d => d.SeverityLevelId)
                .HasConstraintName("tbl_medical_profile_chronic_disease_severity_level_id_fkey");
        });

        modelBuilder.Entity<TblMedicalProfileVaccination>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_profile_vaccination_pkey");

            entity.ToTable("tbl_medical_profile_vaccination");

            entity.HasIndex(e => e.VaccineId, "ix_fk_11489901_tbl_medical_profile_vacci");

            entity.HasIndex(e => new { e.SchoolId, e.UserId }, "ix_tbl_medical_profile_vaccination_school_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VaccineId).HasColumnName("vaccine_id");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.TblMedicalProfileVaccinations)
                .HasForeignKey(d => d.VaccineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_profile_vaccination_vaccine_id_fkey");
        });

        modelBuilder.Entity<TblMedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_record_pkey");

            entity.ToTable("tbl_medical_record");

            entity.HasIndex(e => e.VisitTypeId, "ix_fk_11489428_tbl_medical_record");

            entity.HasIndex(e => e.EmergencyLevelId, "ix_fk_11489433_tbl_medical_record");

            entity.HasIndex(e => new { e.SchoolId, e.TermId }, "ix_tbl_medical_record_school_term");

            entity.HasIndex(e => new { e.SchoolId, e.UserId }, "ix_tbl_medical_record_school_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.DispenserId).HasColumnName("dispenser_id");
            entity.Property(e => e.EmergencyLevelId).HasColumnName("emergency_level_id");
            entity.Property(e => e.LeaveTime).HasColumnName("leave_time");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.SymOther).HasColumnName("sym_other");
            entity.Property(e => e.TermId)
                .HasMaxLength(50)
                .HasColumnName("term_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VisitDate).HasColumnName("visit_date");
            entity.Property(e => e.VisitTime).HasColumnName("visit_time");
            entity.Property(e => e.VisitTypeId).HasColumnName("visit_type_id");

            entity.HasOne(d => d.EmergencyLevel).WithMany(p => p.TblMedicalRecords)
                .HasForeignKey(d => d.EmergencyLevelId)
                .HasConstraintName("tbl_medical_record_emergency_level_id_fkey");

            entity.HasOne(d => d.VisitType).WithMany(p => p.TblMedicalRecords)
                .HasForeignKey(d => d.VisitTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_record_visit_type_id_fkey");
        });

        modelBuilder.Entity<TblMedicalRecordAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_record_attachment_pkey");

            entity.ToTable("tbl_medical_record_attachment");

            entity.HasIndex(e => e.TblMedicalRecordId, "ix_tbl_medical_record_attachment_record");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.FileName).HasColumnName("file_name");
            entity.Property(e => e.FilePath).HasColumnName("file_path");
            entity.Property(e => e.MimeType).HasColumnName("mime_type");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.TblMedicalRecordId).HasColumnName("tbl_medical_record_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.TblMedicalRecord).WithMany(p => p.TblMedicalRecordAttachments)
                .HasForeignKey(d => d.TblMedicalRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_record_attachment_tbl_medical_record_id_fkey");
        });

        modelBuilder.Entity<TblMedicalRecordMedicine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_record_medicine_pkey");

            entity.ToTable("tbl_medical_record_medicine");

            entity.HasIndex(e => e.MedicineId, "ix_fk_11489493_tbl_medical_record_medici");

            entity.HasIndex(e => e.UnitId, "ix_fk_11489498_tbl_medical_record_medici");

            entity.HasIndex(e => e.TblMedicalRecordId, "ix_tbl_medical_record_medicine_record");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.Qty)
                .HasPrecision(10, 2)
                .HasColumnName("qty");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.TblMedicalRecordId).HasColumnName("tbl_medical_record_id");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Medicine).WithMany(p => p.TblMedicalRecordMedicines)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_record_medicine_medicine_id_fkey");

            entity.HasOne(d => d.TblMedicalRecord).WithMany(p => p.TblMedicalRecordMedicines)
                .HasForeignKey(d => d.TblMedicalRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_record_medicine_tbl_medical_record_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.TblMedicalRecordMedicines)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("tbl_medical_record_medicine_unit_id_fkey");
        });

        modelBuilder.Entity<TblMedicalRecordSymptom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_record_symptom_pkey");

            entity.ToTable("tbl_medical_record_symptom");

            entity.HasIndex(e => e.SymptomId, "ix_fk_11489457_tbl_medical_record_sympto");

            entity.HasIndex(e => e.TblMedicalRecordId, "ix_tbl_medical_record_symptom_record");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SymptomId).HasColumnName("symptom_id");
            entity.Property(e => e.TblMedicalRecordId).HasColumnName("tbl_medical_record_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Symptom).WithMany(p => p.TblMedicalRecordSymptoms)
                .HasForeignKey(d => d.SymptomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_record_symptom_symptom_id_fkey");

            entity.HasOne(d => d.TblMedicalRecord).WithMany(p => p.TblMedicalRecordSymptoms)
                .HasForeignKey(d => d.TblMedicalRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_record_symptom_tbl_medical_record_id_fkey");
        });

        modelBuilder.Entity<TblMedicalRecordTreatment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medical_record_treatment_pkey");

            entity.ToTable("tbl_medical_record_treatment");

            entity.HasIndex(e => e.TreatmentId, "ix_fk_11489475_tbl_medical_record_treatm");

            entity.HasIndex(e => e.TblMedicalRecordId, "ix_tbl_medical_record_treatment_record");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.TblMedicalRecordId).HasColumnName("tbl_medical_record_id");
            entity.Property(e => e.TreatmentId).HasColumnName("treatment_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.TblMedicalRecord).WithMany(p => p.TblMedicalRecordTreatments)
                .HasForeignKey(d => d.TblMedicalRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_record_treatment_tbl_medical_record_id_fkey");

            entity.HasOne(d => d.Treatment).WithMany(p => p.TblMedicalRecordTreatments)
                .HasForeignKey(d => d.TreatmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medical_record_treatment_treatment_id_fkey");
        });

        modelBuilder.Entity<TblMedicineUnitMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_medicine_unit_mapping_pkey");

            entity.ToTable("tbl_medicine_unit_mapping");

            entity.HasIndex(e => e.UnitId, "ix_fk_11488973_tbl_medicine_unit_mapping");

            entity.HasIndex(e => e.MedicineId, "ix_tbl_medicine_unit_mapping_medicine");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefaultUnit).HasColumnName("is_default_unit");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SeqNo).HasColumnName("seq_no");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Medicine).WithMany(p => p.TblMedicineUnitMappings)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medicine_unit_mapping_medicine_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.TblMedicineUnitMappings)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tbl_medicine_unit_mapping_unit_id_fkey");
        });

        modelBuilder.Entity<Tbranch>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.BranchId }).HasName("public_TBranch_PK_TBranch");

            entity.ToTable("TBranch");

            entity.HasIndex(e => e.BranchId, "public_TBranch_INDEX_VV_780_779");

            entity.HasIndex(e => e.NTlevel, "public_TBranch_IX_TBranch");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.BranchId).ValueGeneratedOnAdd();
            entity.Property(e => e.BranchName).HasMaxLength(200);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbranchSpec>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.BranchSpecId }).HasName("public_TBranchSpec_PK_TBranchSpec");

            entity.ToTable("TBranchSpec");

            entity.HasIndex(e => e.BranchSpecId, "public_TBranchSpec_INDEX_VV_19312_19311");

            entity.HasIndex(e => e.BranchSubjectId, "public_TBranchSpec_IX_TBranchSpec");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.BranchSpecId).ValueGeneratedOnAdd();
            entity.Property(e => e.BranchSpecName).HasMaxLength(200);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbranchSubject>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.BranchSubjectId }).HasName("public_TBranchSubject_PK_TBranchSubject");

            entity.ToTable("TBranchSubject");

            entity.HasIndex(e => e.BranchSubjectId, "public_TBranchSubject_INDEX_VV_778_777");

            entity.HasIndex(e => e.BranchId, "public_TBranchSubject_IX_TBranchSubject");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.BranchSubjectId).ValueGeneratedOnAdd();
            entity.Property(e => e.BranchSubjectName).HasMaxLength(200);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TbstudentViewSyncQueue>(entity =>
        {
            entity.HasKey(e => e.QueueId).HasName("TBStudentViewSyncQueue_pkey");

            entity.ToTable("TBStudentViewSyncQueue");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "idx_syncqueue_school_sid_unprocessed").HasFilter("(\"Processed\" = false)");

            entity.Property(e => e.QueueId).HasColumnName("QueueID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Processed).HasDefaultValue(false);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<Tclass>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SClassId }).HasName("public_TClass_PK_TClass");

            entity.ToTable("TClass");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SClassId)
                .HasMaxLength(10)
                .HasColumnName("sClassID");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SClass)
                .HasMaxLength(250)
                .HasColumnName("sClass");
            entity.Property(e => e.SClassIp)
                .HasMaxLength(15)
                .HasColumnName("sClassIP");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TclassMember>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NClassMemberid }).HasName("public_TClassMember_PK_Table_2");

            entity.ToTable("TClassMember");

            entity.HasIndex(e => new { e.NTermSubLevel2, e.NTerm, e.SchoolId }, "public_TClassMember_IDX_M_1448_1447");

            entity.HasIndex(e => new { e.NTermSubLevel2, e.SchoolId }, "public_TClassMember_IDX_M_206765_206764");

            entity.HasIndex(e => new { e.NTeacherHeadid, e.NTermSubLevel2, e.NTerm, e.SchoolId }, "public_TClassMember_IDX_M_4084_4083");

            entity.HasIndex(e => new { e.NTeacherAssistTwo, e.SchoolId }, "public_TClassMember_INDEX_VV_1505_1504");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "public_TClassMember_INDEX_VV_207_206");

            entity.HasIndex(e => e.NTerm, "public_TClassMember_INDEX_VV_89574_89573");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.NTermSubLevel2, e.NTeacherHeadid, e.NTeacherAssistOne, e.NTeacherAssistTwo }, "public_TClassMember_IX_TClassMember_Active_0");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.NTermSubLevel2, e.NTeacherHeadid, e.NTeacherAssistOne, e.NTeacherAssistTwo }, "public_TClassMember_IX_TClassMember_Active_NULL");

            entity.HasIndex(e => new { e.SchoolId, e.NTeacherHeadid, e.NTeacherAssistOne, e.NTeacherAssistTwo, e.NTerm, e.NTermSubLevel2 }, "public_TClassMember_IX_TClassMember_Approval");

            entity.HasIndex(e => new { e.SchoolId, e.NTeacherHeadid, e.NTeacherAssistOne, e.NTeacherAssistTwo, e.NTerm, e.NTermSubLevel2 }, "public_TClassMember_IX_TClassMember_ApprovalNULL");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NClassMemberid)
                .ValueGeneratedOnAdd()
                .HasColumnName("nClassMemberid");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTeacherAssistOne).HasColumnName("nTeacherAssistOne");
            entity.Property(e => e.NTeacherAssistTwo).HasColumnName("nTeacherAssistTwo");
            entity.Property(e => e.NTeacherHeadid).HasColumnName("nTeacherHeadid");
            entity.Property(e => e.NTerm)
                .HasMaxLength(50)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TclassOnline>(entity =>
        {
            entity.HasKey(e => e.OnlineId).HasName("public_TClassOnline_PK_TClassOnline");

            entity.ToTable("TClassOnline");

            entity.HasIndex(e => new { e.TermId, e.LevelId, e.SchoolId }, "public_TClassOnline_IDX_M_138_137");

            entity.HasIndex(e => new { e.TermId, e.LevelId, e.PlanId, e.SchoolId }, "public_TClassOnline_IDX_M_3183_3182");

            entity.HasIndex(e => e.SchoolId, "public_TClassOnline_INDEX_VV_98267_98266");

            entity.HasIndex(e => e.TitleName, "public_TClassOnline_TClassOnline_PlanID_SchoolID");

            entity.HasIndex(e => e.PlanId, "public_TClassOnline_TClassOnline_SchoolID");

            entity.HasIndex(e => e.PlanId, "public_TClassOnline_TClassOnline_SchoolID1");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Modfied).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SelectedRoom).HasMaxLength(150);
            entity.Property(e => e.ShareId).HasMaxLength(250);
            entity.Property(e => e.TermId).HasMaxLength(20);
            entity.Property(e => e.TitleName).HasMaxLength(150);
        });

        modelBuilder.Entity<Tcompany>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TCompany", "masterdb_schema");

            entity.Property(e => e.ActiveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Admin1).HasColumnName("admin1");
            entity.Property(e => e.Admin2).HasColumnName("admin2");
            entity.Property(e => e.AumpherId).HasColumnName("AumpherID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Checker).HasColumnName("checker");
            entity.Property(e => e.CostGradeRepair).HasPrecision(6, 2);
            entity.Property(e => e.CostGradeRepairFinal).HasPrecision(6, 2);
            entity.Property(e => e.CostGradeRepairMid).HasPrecision(6, 2);
            entity.Property(e => e.CrmGrade).HasMaxLength(1);
            entity.Property(e => e.DoubleAuthen).HasColumnName("double_authen");
            entity.Property(e => e.EducationArea).HasMaxLength(100);
            entity.Property(e => e.InactiveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.IsActiveSendMessageToLine).HasColumnName("IsActiveSendMessageToLINE");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.NAcademicDirectorid).HasColumnName("nAcademicDirectorid");
            entity.Property(e => e.NAcademicSubDirectorid).HasColumnName("nAcademicSubDirectorid");
            entity.Property(e => e.NAccountingDirectorid).HasColumnName("nAccountingDirectorid");
            entity.Property(e => e.NCompany).HasColumnName("nCompany");
            entity.Property(e => e.NGm).HasColumnName("nGM");
            entity.Property(e => e.NPersonnel).HasColumnName("nPersonnel");
            entity.Property(e => e.NRegistraDirectorid).HasColumnName("nRegistraDirectorid");
            entity.Property(e => e.NSchoolHeadid).HasColumnName("nSchoolHeadid");
            entity.Property(e => e.NStudentDevelopmentDirectorid).HasColumnName("nStudentDevelopmentDirectorid");
            entity.Property(e => e.NType)
                .HasMaxLength(1)
                .HasColumnName("nType");
            entity.Property(e => e.NWebAdminid).HasColumnName("nWebAdminid");
            entity.Property(e => e.Nfc).HasColumnName("nfc");
            entity.Property(e => e.PaymentApiurl)
                .HasMaxLength(400)
                .HasColumnName("PaymentAPIUrl");
            entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");
            entity.Property(e => e.SAddress)
                .HasMaxLength(250)
                .HasColumnName("sAddress");
            entity.Property(e => e.SAumpher)
                .HasMaxLength(100)
                .HasColumnName("sAumpher");
            entity.Property(e => e.SAumpherEng)
                .HasMaxLength(100)
                .HasColumnName("sAumpherEng");
            entity.Property(e => e.SCode)
                .HasMaxLength(50)
                .HasColumnName("sCode");
            entity.Property(e => e.SCompany)
                .HasMaxLength(250)
                .HasColumnName("sCompany");
            entity.Property(e => e.SDatabases)
                .HasMaxLength(100)
                .HasColumnName("sDatabases");
            entity.Property(e => e.SEmailOne)
                .HasMaxLength(100)
                .HasColumnName("sEmailOne");
            entity.Property(e => e.SEmailTwo)
                .HasMaxLength(100)
                .HasColumnName("sEmailTwo");
            entity.Property(e => e.SEntities)
                .HasMaxLength(100)
                .HasColumnName("sEntities");
            entity.Property(e => e.SFax)
                .HasMaxLength(50)
                .HasColumnName("sFax");
            entity.Property(e => e.SHomeNumber)
                .HasMaxLength(100)
                .HasColumnName("sHomeNumber");
            entity.Property(e => e.SImage)
                .HasMaxLength(1000)
                .HasColumnName("sImage");
            entity.Property(e => e.SMobilePhone)
                .HasMaxLength(50)
                .HasColumnName("sMobilePhone");
            entity.Property(e => e.SMuu)
                .HasMaxLength(100)
                .HasColumnName("sMuu");
            entity.Property(e => e.SNameEn)
                .HasMaxLength(100)
                .HasColumnName("sNameEN");
            entity.Property(e => e.SOwner)
                .HasMaxLength(100)
                .HasColumnName("sOwner");
            entity.Property(e => e.SPassword)
                .HasMaxLength(100)
                .HasColumnName("sPassword");
            entity.Property(e => e.SPhoneOne)
                .HasMaxLength(50)
                .HasColumnName("sPhoneOne");
            entity.Property(e => e.SPhoneTwo)
                .HasMaxLength(50)
                .HasColumnName("sPhoneTwo");
            entity.Property(e => e.SPost)
                .HasMaxLength(100)
                .HasColumnName("sPost");
            entity.Property(e => e.SProvince)
                .HasMaxLength(50)
                .HasColumnName("sProvince");
            entity.Property(e => e.SProvinceEng)
                .HasMaxLength(50)
                .HasColumnName("sProvinceEng");
            entity.Property(e => e.SRoad)
                .HasMaxLength(100)
                .HasColumnName("sRoad");
            entity.Property(e => e.SSchoolHistory).HasColumnName("sSchoolHistory");
            entity.Property(e => e.SSchoolMission).HasColumnName("sSchoolMission");
            entity.Property(e => e.SSchoolPass)
                .HasMaxLength(100)
                .HasColumnName("sSchoolPass");
            entity.Property(e => e.SSchoolVision).HasColumnName("sSchoolVision");
            entity.Property(e => e.SServer)
                .HasMaxLength(100)
                .HasColumnName("sServer");
            entity.Property(e => e.SShortSchoolName)
                .HasMaxLength(10)
                .HasColumnName("sShortSchoolName");
            entity.Property(e => e.SSoy)
                .HasMaxLength(100)
                .HasColumnName("sSoy");
            entity.Property(e => e.STel)
                .HasMaxLength(15)
                .HasColumnName("sTel");
            entity.Property(e => e.STumbon)
                .HasMaxLength(100)
                .HasColumnName("sTumbon");
            entity.Property(e => e.SUser)
                .HasMaxLength(100)
                .HasColumnName("sUser");
            entity.Property(e => e.SWebsite)
                .HasMaxLength(100)
                .HasColumnName("sWebsite");
            entity.Property(e => e.SchoolAreaCode).HasMaxLength(10);
            entity.Property(e => e.SchoolClass).HasMaxLength(150);
            entity.Property(e => e.SchoolCoverPicture)
                .HasMaxLength(250)
                .HasColumnName("schoolCoverPicture");
            entity.Property(e => e.SchoolDataType).HasMaxLength(50);
            entity.Property(e => e.SchoolGrade)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.SchoolHeadEmail).HasMaxLength(100);
            entity.Property(e => e.SchoolHeadLastname).HasMaxLength(100);
            entity.Property(e => e.SchoolHeadName).HasMaxLength(100);
            entity.Property(e => e.SchoolHeadPhone).HasMaxLength(100);
            entity.Property(e => e.SchoolHeadPicture)
                .HasMaxLength(250)
                .HasColumnName("schoolHeadPicture");
            entity.Property(e => e.ServerAdminEmail).HasMaxLength(100);
            entity.Property(e => e.ServerAdminLastname).HasMaxLength(100);
            entity.Property(e => e.ServerAdminName).HasMaxLength(100);
            entity.Property(e => e.ServerAdminPhone).HasMaxLength(100);
            entity.Property(e => e.ServerAdminPicture)
                .HasMaxLength(250)
                .HasColumnName("serverAdminPicture");
            entity.Property(e => e.SettingGradeAdmin).HasColumnName("settingGradeAdmin");
            entity.Property(e => e.SettingPlanTeacher).HasColumnName("settingPlanTeacher");
            entity.Property(e => e.SettingReportView).HasColumnName("settingReportView");
            entity.Property(e => e.SettingTimePeriod).HasColumnName("settingTimePeriod");
            entity.Property(e => e.SingleAuthen).HasColumnName("single_authen");
            entity.Property(e => e.Sotfware).HasColumnName("sotfware");
            entity.Property(e => e.TaxId).HasMaxLength(100);
            entity.Property(e => e.TumbonId).HasColumnName("TumbonID");
            entity.Property(e => e.UId).HasColumnName("uID");
            entity.Property(e => e.UserSaleId).HasColumnName("UserSaleID");
            entity.Property(e => e.UserSupportId).HasColumnName("UserSupportID");
        });

        modelBuilder.Entity<Tcompany1>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TCompany");

            entity.Property(e => e.ActiveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Admin1).HasColumnName("admin1");
            entity.Property(e => e.Admin2).HasColumnName("admin2");
            entity.Property(e => e.AumpherId).HasColumnName("AumpherID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Checker).HasColumnName("checker");
            entity.Property(e => e.CostGradeRepair).HasPrecision(6, 2);
            entity.Property(e => e.CostGradeRepairFinal).HasPrecision(6, 2);
            entity.Property(e => e.CostGradeRepairMid).HasPrecision(6, 2);
            entity.Property(e => e.CrmGrade).HasMaxLength(1);
            entity.Property(e => e.DoubleAuthen).HasColumnName("double_authen");
            entity.Property(e => e.EducationArea).HasMaxLength(100);
            entity.Property(e => e.InactiveDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.IsActiveSendMessageToLine).HasColumnName("IsActiveSendMessageToLINE");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.NAcademicDirectorid).HasColumnName("nAcademicDirectorid");
            entity.Property(e => e.NAcademicSubDirectorid).HasColumnName("nAcademicSubDirectorid");
            entity.Property(e => e.NAccountingDirectorid).HasColumnName("nAccountingDirectorid");
            entity.Property(e => e.NCompany).HasColumnName("nCompany");
            entity.Property(e => e.NGm).HasColumnName("nGM");
            entity.Property(e => e.NPersonnel).HasColumnName("nPersonnel");
            entity.Property(e => e.NRegistraDirectorid).HasColumnName("nRegistraDirectorid");
            entity.Property(e => e.NSchoolHeadid).HasColumnName("nSchoolHeadid");
            entity.Property(e => e.NStudentDevelopmentDirectorid).HasColumnName("nStudentDevelopmentDirectorid");
            entity.Property(e => e.NType)
                .HasMaxLength(1)
                .HasColumnName("nType");
            entity.Property(e => e.NWebAdminid).HasColumnName("nWebAdminid");
            entity.Property(e => e.Nfc).HasColumnName("nfc");
            entity.Property(e => e.PaymentApiurl)
                .HasMaxLength(400)
                .HasColumnName("PaymentAPIUrl");
            entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");
            entity.Property(e => e.SAddress)
                .HasMaxLength(250)
                .HasColumnName("sAddress");
            entity.Property(e => e.SAumpher)
                .HasMaxLength(100)
                .HasColumnName("sAumpher");
            entity.Property(e => e.SAumpherEng)
                .HasMaxLength(100)
                .HasColumnName("sAumpherEng");
            entity.Property(e => e.SCode)
                .HasMaxLength(50)
                .HasColumnName("sCode");
            entity.Property(e => e.SCompany)
                .HasMaxLength(250)
                .HasColumnName("sCompany");
            entity.Property(e => e.SDatabases)
                .HasMaxLength(100)
                .HasColumnName("sDatabases");
            entity.Property(e => e.SEmailOne)
                .HasMaxLength(100)
                .HasColumnName("sEmailOne");
            entity.Property(e => e.SEmailTwo)
                .HasMaxLength(100)
                .HasColumnName("sEmailTwo");
            entity.Property(e => e.SEntities)
                .HasMaxLength(100)
                .HasColumnName("sEntities");
            entity.Property(e => e.SFax)
                .HasMaxLength(50)
                .HasColumnName("sFax");
            entity.Property(e => e.SHomeNumber)
                .HasMaxLength(100)
                .HasColumnName("sHomeNumber");
            entity.Property(e => e.SImage)
                .HasMaxLength(1000)
                .HasColumnName("sImage");
            entity.Property(e => e.SMobilePhone)
                .HasMaxLength(50)
                .HasColumnName("sMobilePhone");
            entity.Property(e => e.SMuu)
                .HasMaxLength(100)
                .HasColumnName("sMuu");
            entity.Property(e => e.SNameEn)
                .HasMaxLength(100)
                .HasColumnName("sNameEN");
            entity.Property(e => e.SOwner)
                .HasMaxLength(100)
                .HasColumnName("sOwner");
            entity.Property(e => e.SPassword)
                .HasMaxLength(100)
                .HasColumnName("sPassword");
            entity.Property(e => e.SPhoneOne)
                .HasMaxLength(50)
                .HasColumnName("sPhoneOne");
            entity.Property(e => e.SPhoneTwo)
                .HasMaxLength(50)
                .HasColumnName("sPhoneTwo");
            entity.Property(e => e.SPost)
                .HasMaxLength(100)
                .HasColumnName("sPost");
            entity.Property(e => e.SProvince)
                .HasMaxLength(50)
                .HasColumnName("sProvince");
            entity.Property(e => e.SProvinceEng)
                .HasMaxLength(50)
                .HasColumnName("sProvinceEng");
            entity.Property(e => e.SRoad)
                .HasMaxLength(100)
                .HasColumnName("sRoad");
            entity.Property(e => e.SSchoolHistory).HasColumnName("sSchoolHistory");
            entity.Property(e => e.SSchoolMission).HasColumnName("sSchoolMission");
            entity.Property(e => e.SSchoolPass)
                .HasMaxLength(100)
                .HasColumnName("sSchoolPass");
            entity.Property(e => e.SSchoolVision).HasColumnName("sSchoolVision");
            entity.Property(e => e.SServer)
                .HasMaxLength(100)
                .HasColumnName("sServer");
            entity.Property(e => e.SShortSchoolName)
                .HasMaxLength(10)
                .HasColumnName("sShortSchoolName");
            entity.Property(e => e.SSoy)
                .HasMaxLength(100)
                .HasColumnName("sSoy");
            entity.Property(e => e.STel)
                .HasMaxLength(15)
                .HasColumnName("sTel");
            entity.Property(e => e.STumbon)
                .HasMaxLength(100)
                .HasColumnName("sTumbon");
            entity.Property(e => e.SUser)
                .HasMaxLength(100)
                .HasColumnName("sUser");
            entity.Property(e => e.SWebsite)
                .HasMaxLength(100)
                .HasColumnName("sWebsite");
            entity.Property(e => e.SchoolAreaCode).HasMaxLength(10);
            entity.Property(e => e.SchoolClass).HasMaxLength(150);
            entity.Property(e => e.SchoolCoverPicture)
                .HasMaxLength(250)
                .HasColumnName("schoolCoverPicture");
            entity.Property(e => e.SchoolDataType).HasMaxLength(50);
            entity.Property(e => e.SchoolGrade)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.SchoolHeadEmail).HasMaxLength(100);
            entity.Property(e => e.SchoolHeadLastname).HasMaxLength(100);
            entity.Property(e => e.SchoolHeadName).HasMaxLength(100);
            entity.Property(e => e.SchoolHeadPhone).HasMaxLength(100);
            entity.Property(e => e.SchoolHeadPicture)
                .HasMaxLength(250)
                .HasColumnName("schoolHeadPicture");
            entity.Property(e => e.ServerAdminEmail).HasMaxLength(100);
            entity.Property(e => e.ServerAdminLastname).HasMaxLength(100);
            entity.Property(e => e.ServerAdminName).HasMaxLength(100);
            entity.Property(e => e.ServerAdminPhone).HasMaxLength(100);
            entity.Property(e => e.ServerAdminPicture)
                .HasMaxLength(250)
                .HasColumnName("serverAdminPicture");
            entity.Property(e => e.SettingGradeAdmin).HasColumnName("settingGradeAdmin");
            entity.Property(e => e.SettingPlanTeacher).HasColumnName("settingPlanTeacher");
            entity.Property(e => e.SettingReportView).HasColumnName("settingReportView");
            entity.Property(e => e.SettingTimePeriod).HasColumnName("settingTimePeriod");
            entity.Property(e => e.SingleAuthen).HasColumnName("single_authen");
            entity.Property(e => e.Sotfware).HasColumnName("sotfware");
            entity.Property(e => e.TaxId).HasMaxLength(100);
            entity.Property(e => e.TumbonId).HasColumnName("TumbonID");
            entity.Property(e => e.UId).HasColumnName("uID");
            entity.Property(e => e.UserSaleId).HasColumnName("UserSaleID");
            entity.Property(e => e.UserSupportId).HasColumnName("UserSupportID");
        });

        modelBuilder.Entity<TcourseAbbreviation>(entity =>
        {
            entity.HasKey(e => e.CourseAbbreviationId).HasName("public_TCourseAbbreviation_PK__TCourseA__FFD92FCC08290B1D");

            entity.ToTable("TCourseAbbreviation");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ShortName).HasMaxLength(150);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TcourseGroup>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.CourseGroupId }).HasName("public_TCourseGroup_PK_TCourseGroup");

            entity.ToTable("TCourseGroup");

            entity.HasIndex(e => e.CourseGroupId, "public_TCourseGroup_INDEX_VV_89878_89877");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CourseGroupId)
                .ValueGeneratedOnAdd()
                .HasColumnName("courseGroupId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Description).HasMaxLength(30);
            entity.Property(e => e.DescriptionEn).HasMaxLength(100);
            entity.Property(e => e.MasterCode).HasMaxLength(5);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TcourseType>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.CourseTypeId }).HasName("public_TCourseType_PK_TCourseType");

            entity.ToTable("TCourseType");

            entity.HasIndex(e => e.CourseTypeId, "public_TCourseType_INDEX_VV_12040_12039");

            entity.HasIndex(e => e.CourseTypeId, "public_TCourseType_IX_TCourseType_Optim");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CourseTypeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("courseTypeId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.DescriptionEn).HasMaxLength(100);
            entity.Property(e => e.MasterCode).HasMaxLength(5);
            entity.Property(e => e.NOrder).HasColumnName("nOrder");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Tcurriculum>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.CurriculumId }).HasName("public_TCurriculum_PK_TCurriculum");

            entity.ToTable("TCurriculum");

            entity.HasIndex(e => e.NYear, "public_TCurriculum_IX_TCurriculum");

            entity.HasIndex(e => e.CurriculumId, "public_TCurriculum_IX_TCurriculum_CurriculumId");

            entity.Property(e => e.CurriculumId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CurriculumName).HasMaxLength(250);
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TcurriculumLink>(entity =>
        {
            entity.HasKey(e => e.CurriculumLinkId);

            entity.ToTable("TCurriculumLink");

            entity.Property(e => e.CurriculumLinkId).UseIdentityAlwaysColumn();
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Nterm)
                .HasMaxLength(20)
                .HasColumnName("NTerm");
            entity.Property(e => e.NtsubLevel).HasColumnName("NTSubLevel");
            entity.Property(e => e.Nyear).HasColumnName("NYear");
        });

        modelBuilder.Entity<TcurriculumLinkCompetencySubject>(entity =>
        {
            entity.HasKey(e => e.CurriculumLinkCompetencySubjectId);

            entity.ToTable("TCurriculumLinkCompetencySubject");

            entity.Property(e => e.CurriculumLinkCompetencySubjectId).UseIdentityAlwaysColumn();
            entity.Property(e => e.CompetencySplaneId).HasColumnName("CompetencySPlaneId");
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FinalWeightPercent).HasPrecision(5, 2);
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StudyWeightPercent).HasPrecision(5, 2);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TcurriculumLinkCourseType>(entity =>
        {
            entity.HasKey(e => e.CurriculumLinkCourseTypeId);

            entity.ToTable("TCurriculumLinkCourseType");

            entity.Property(e => e.CurriculumLinkCourseTypeId).UseIdentityAlwaysColumn();
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FinalWeightPercent).HasPrecision(5, 2);
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StudyWeightPercent).HasPrecision(5, 2);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TcurriculumLinkSubjectMapping>(entity =>
        {
            entity.HasKey(e => e.CurriculumLinkSubjectMappingId);

            entity.ToTable("TCurriculumLinkSubjectMapping");

            entity.Property(e => e.CurriculumLinkSubjectMappingId).UseIdentityAlwaysColumn();
            entity.Property(e => e.CompetencySplaneId).HasColumnName("CompetencySPlaneId");
            entity.Property(e => e.Core2008SplaneId).HasColumnName("Core2008SPlaneId");
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FinalWeightPercent)
                .HasPrecision(5, 2)
                .HasColumnName("FinalWeightPercent ");
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StudyWeightPercent).HasPrecision(5, 2);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Tdepartment>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.DepId }).HasName("public_TDepartment_PK_TDepartment");

            entity.ToTable("TDepartment");

            entity.HasIndex(e => e.DepId, "public_TDepartment_INDEX_VV_125767_125766");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.DepId)
                .ValueGeneratedOnAdd()
                .HasColumnName("DepID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(50)
                .HasColumnName("departmentName");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UserApproveOne).HasColumnName("userApproveOne");
            entity.Property(e => e.UserApproveTwo).HasColumnName("userApproveTwo");
            entity.Property(e => e.UserHeadId).HasColumnName("userHeadId");
        });

        modelBuilder.Entity<TdocumentStampDate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_TDocumentStampDate_PK_TDocumentStampDate");

            entity.ToTable("TDocumentStampDate");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DocDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DocType).HasMaxLength(3);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TempAddress>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpAddress_PK_TEMPADDRESS");

            entity.ToTable("TEmpAddress");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Alley).HasMaxLength(40);
            entity.Property(e => e.Building).HasMaxLength(50);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DistrictId).HasColumnName("DistrictID");
            entity.Property(e => e.No).HasMaxLength(10);
            entity.Property(e => e.Postcode).HasMaxLength(10);
            entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");
            entity.Property(e => e.Road).HasMaxLength(40);
            entity.Property(e => e.SubdistrictId).HasColumnName("SubdistrictID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Village).HasMaxLength(50);
            entity.Property(e => e.VillageNo).HasMaxLength(10);
        });

        modelBuilder.Entity<TempCard>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TempCard");

            entity.Property(e => e.BarCode).HasMaxLength(20);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CardHistoryId).HasColumnName("CardHistoryID");
            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.CardName).HasMaxLength(250);
            entity.Property(e => e.Nfc)
                .HasMaxLength(20)
                .HasColumnName("NFC");
            entity.Property(e => e.Nfcencrypt)
                .HasMaxLength(20)
                .HasColumnName("NFCEncrypt");
            entity.Property(e => e.NfcencryptReverse)
                .HasMaxLength(20)
                .HasColumnName("NFCEncryptReverse");
            entity.Property(e => e.Nfcreverse)
                .HasMaxLength(20)
                .HasColumnName("NFCReverse");
            entity.Property(e => e.ReturnDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<TempEducationInfo>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpEducationInfo_PK_TEMPEDUCATIONINFO");

            entity.ToTable("TEmpEducationInfo");

            entity.HasIndex(e => new { e.SEmp, e.CDel }, "public_TEmpEducationInfo_IX_TEmpEducationInfo_sEmp_cDel");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Institution).HasMaxLength(100);
            entity.Property(e => e.LevelId).HasColumnName("LevelID");
            entity.Property(e => e.Major).HasMaxLength(100);
            entity.Property(e => e.MinorSubject).HasMaxLength(100);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TempFamily>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpFamily_PK_TEMPFAMILY");

            entity.ToTable("TEmpFamily");

            entity.HasIndex(e => new { e.SEmp, e.CDel }, "public_TEmpFamily_INDEX_VV_18226_18225");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Birthday).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DeathStatus).HasMaxLength(30);
            entity.Property(e => e.FamilyCareer).HasMaxLength(50);
            entity.Property(e => e.FamilyRelation).HasMaxLength(20);
            entity.Property(e => e.FirstName).HasMaxLength(30);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.LevelId).HasColumnName("LevelID");
            entity.Property(e => e.LiveStatus).HasMaxLength(30);
            entity.Property(e => e.TitleId).HasColumnName("TitleID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TempHonor>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpHonor_PK_TEMPHONOR");

            entity.ToTable("TEmpHonor");

            entity.HasIndex(e => new { e.SEmp, e.CDel }, "public_TEmpHonor_IX_TEmpHonor_sEmp_cDel");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TempInsignium>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpInsignia_PK_TEMPINSIGNIA");

            entity.ToTable("TEmpInsignia");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookNumber).HasMaxLength(10);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Date).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Duty).HasMaxLength(10);
            entity.Property(e => e.Grade).HasMaxLength(50);
            entity.Property(e => e.Number).HasMaxLength(10);
            entity.Property(e => e.Part).HasMaxLength(10);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TempProfessionalLicense>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpProfessionalLicense_PK_TEMPPROFESSIONALLICENSE");

            entity.ToTable("TEmpProfessionalLicense");

            entity.HasIndex(e => new { e.SEmp, e.CDel }, "public_TEmpProfessionalLicense_IX_TEmpProfessionalLicense_sEmp_");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AgencyIssued).HasMaxLength(100);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ExpireDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.IssuedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LicenseName).HasMaxLength(100);
            entity.Property(e => e.LicenseNo).HasMaxLength(20);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TempSalary>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpSalary_PK_TEMPSALARY");

            entity.ToTable("TEmpSalary");

            entity.HasIndex(e => new { e.WorkStatus, e.SchoolId }, "public_TEmpSalary_INDEX_VV_145730_145729");

            entity.HasIndex(e => e.SEmp, "public_TEmpSalary_INDEX_VV_146515_146514");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "public_TEmpSalary_INDEX_VV_22632_22631");

            entity.HasIndex(e => new { e.SEmp, e.WorkStatus }, "public_TEmpSalary_IX_TEmpSalary_sEmp_WorkStatus");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AcademicStandingMoney).HasPrecision(8, 2);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Degree).HasMaxLength(100);
            entity.Property(e => e.GovernmentOrderDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NetSalary).HasPrecision(8, 2);
            entity.Property(e => e.PositionMoney).HasPrecision(8, 2);
            entity.Property(e => e.RetirementDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Salary).HasPrecision(8, 2);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.WorkInEducationDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.WorkStartDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TempSigner>(entity =>
        {
            entity.HasKey(e => new { e.SignerId, e.SchoolId }).HasName("public_TEmpSigner_PK_TEmpSigner");

            entity.ToTable("TEmpSigner");

            entity.HasIndex(e => new { e.SchoolId, e.IsDel }, "public_TEmpSigner_INDEX_VV_102695_102694");

            entity.HasIndex(e => e.YearId, "public_TEmpSigner_INDEX_VV_120207_120206");

            entity.HasIndex(e => e.PositionEn, "public_TEmpSigner_INDEX_VV_695_694");

            entity.Property(e => e.SignerId)
                .ValueGeneratedOnAdd()
                .HasColumnName("SignerID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DefaultId).HasColumnName("DefaultID");
            entity.Property(e => e.EmpId).HasColumnName("EmpID");
            entity.Property(e => e.Modified).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.OldSigner).HasMaxLength(250);
            entity.Property(e => e.Position).HasMaxLength(250);
            entity.Property(e => e.PositionEn)
                .HasMaxLength(250)
                .HasColumnName("PositionEN");
            entity.Property(e => e.Signer).HasMaxLength(250);
            entity.Property(e => e.TermId)
                .HasMaxLength(20)
                .HasColumnName("TermID");
            entity.Property(e => e.YearId).HasColumnName("YearID");
        });

        modelBuilder.Entity<TempTeaching>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpTeaching_PK_TEMPTEACHING");

            entity.ToTable("TEmpTeaching");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CourseTypeId).HasColumnName("courseTypeId");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.SClassId)
                .HasMaxLength(10)
                .HasColumnName("sClassID");
            entity.Property(e => e.SRoomId)
                .HasMaxLength(10)
                .HasColumnName("sRoomID");
            entity.Property(e => e.SubjectId).HasColumnName("SUBJECT_ID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TempToeic>(entity =>
        {
            entity.HasKey(e => new { e.SEmp, e.Id, e.SchoolId }).HasName("public_TEmpTOEIC_PK_TEmpTOEIC");

            entity.ToTable("TEmpTOEIC");

            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ExpirationDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.InstitutionAnnouncement).HasMaxLength(300);
            entity.Property(e => e.Toeicscore).HasColumnName("TOEICScore");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TempTraining>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmpTraining_PK_TEMPTRAINING");

            entity.ToTable("TEmpTraining");

            entity.HasIndex(e => new { e.SEmp, e.CDel }, "public_TEmpTraining_IDX_M_53_52");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EndDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Expenses).HasPrecision(6, 2);
            entity.Property(e => e.Place).HasMaxLength(100);
            entity.Property(e => e.ProjectName).HasMaxLength(100);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");
            entity.Property(e => e.StartDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TrainingHours).HasPrecision(6, 2);
            entity.Property(e => e.TrainingName).HasMaxLength(100);
            entity.Property(e => e.TrainingType).HasMaxLength(2);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Temployee>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp }).HasName("public_TEmployees_PK_TEmployees");

            entity.ToTable("TEmployees");

            entity.HasIndex(e => new { e.SchoolId, e.NMoney }, "IDX_TEmployees_SchoolID_nMoney")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.SPhone }, "IDX_TEmployees_SchoolID_sPhone")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.SchoolId }, "IDX_TEmployees_cDel_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.CType, e.SchoolId }, "IDX_TEmployees_cDel_cType_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.GradeSystemAdmin, e.SchoolId }, "IDX_TEmployees_cDel_gradeSystemAdmin_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CType, e.SchoolId }, "IDX_TEmployees_cType_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.NDepartmentId, e.SchoolId }, "IDX_TEmployees_nDepartmentId_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.SEmp, "IDX_TEmployees_sEmp")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SIdentification, e.CDel, e.SchoolId, e.SEmp }, "IDX_TEmployees_sIdentification_cDel_SchoolID_sEmp")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.SName, "IDX_TEmployees_sName")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.SchoolId, "idx_temployees_schoolid");

            entity.HasIndex(e => e.SchoolId, "idx_temployees_schoolid_cdel").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.HasIndex(e => e.SEmp, "idx_temployees_semp").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.HasIndex(e => e.SEmp, "idx_temployees_semp_active").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.HasIndex(e => new { e.SEmp, e.CDel }, "idx_temployees_semp_cdel");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Building).HasMaxLength(50);
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CSex)
                .HasMaxLength(1)
                .HasColumnName("cSex");
            entity.Property(e => e.CType)
                .HasMaxLength(10)
                .HasColumnName("cType");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DBirth)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dBirth");
            entity.Property(e => e.DPicUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dPicUpdate");
            entity.Property(e => e.DUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dUpdate");
            entity.Property(e => e.DailyUsageLimit).HasPrecision(18, 2);
            entity.Property(e => e.GradeSystemAdmin).HasColumnName("gradeSystemAdmin");
            entity.Property(e => e.Leavecheck).HasColumnName("leavecheck");
            entity.Property(e => e.NDepartmentId).HasColumnName("nDepartmentId");
            entity.Property(e => e.NJobid).HasColumnName("nJobid");
            entity.Property(e => e.NMax)
                .HasPrecision(18, 2)
                .HasColumnName("nMax");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 2)
                .HasColumnName("nMoney");
            entity.Property(e => e.NPicversion).HasColumnName("nPicversion");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.SAddress)
                .HasMaxLength(512)
                .HasColumnName("sAddress");
            entity.Property(e => e.SAumpher)
                .HasMaxLength(5)
                .HasColumnName("sAumpher");
            entity.Property(e => e.SAutoBillingEmail)
                .HasMaxLength(256)
                .HasColumnName("sAutoBillingEmail");
            entity.Property(e => e.SCity)
                .HasMaxLength(512)
                .HasColumnName("sCity");
            entity.Property(e => e.SClaim)
                .HasMaxLength(100)
                .HasColumnName("sClaim");
            entity.Property(e => e.SCountry)
                .HasMaxLength(512)
                .HasColumnName("sCountry");
            entity.Property(e => e.SEmail)
                .HasMaxLength(256)
                .HasColumnName("sEmail");
            entity.Property(e => e.SFinger).HasColumnName("sFinger");
            entity.Property(e => e.SFinger2).HasColumnName("sFinger2");
            entity.Property(e => e.SHomeNumber)
                .HasMaxLength(100)
                .HasColumnName("sHomeNumber");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastNameEng)
                .HasMaxLength(256)
                .HasColumnName("sLastNameEng");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SMuu)
                .HasMaxLength(50)
                .HasColumnName("sMuu");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNameEng)
                .HasMaxLength(256)
                .HasColumnName("sNameEng");
            entity.Property(e => e.SPassword)
                .HasMaxLength(100)
                .HasColumnName("sPassword");
            entity.Property(e => e.SPhone)
                .HasMaxLength(20)
                .HasColumnName("sPhone");
            entity.Property(e => e.SPicture)
                .HasMaxLength(250)
                .HasColumnName("sPicture");
            entity.Property(e => e.SPost)
                .HasMaxLength(10)
                .HasColumnName("sPost");
            entity.Property(e => e.SPostalcode)
                .HasMaxLength(10)
                .HasColumnName("sPostalcode");
            entity.Property(e => e.SProvince)
                .HasMaxLength(5)
                .HasColumnName("sProvince");
            entity.Property(e => e.SRoad)
                .HasMaxLength(50)
                .HasColumnName("sRoad");
            entity.Property(e => e.SSoy)
                .HasMaxLength(50)
                .HasColumnName("sSoy");
            entity.Property(e => e.SStatusReport)
                .HasMaxLength(20)
                .HasColumnName("sStatusReport");
            entity.Property(e => e.SSubtopic)
                .HasMaxLength(512)
                .HasColumnName("sSubtopic");
            entity.Property(e => e.STitle)
                .HasMaxLength(10)
                .HasColumnName("sTitle");
            entity.Property(e => e.SToken).HasColumnName("sToken");
            entity.Property(e => e.STumbon)
                .HasMaxLength(5)
                .HasColumnName("sTumbon");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Village).HasMaxLength(50);
        });

        modelBuilder.Entity<TemployeeInfo>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SEmp, e.Id }).HasName("public_TEmployeeInfo_PK_TEMPLOYEEINFO");

            entity.ToTable("TEmployeeInfo");

            entity.HasIndex(e => e.SEmp, "public_TEmployeeInfo_IDX_M_27937_27936");

            entity.HasIndex(e => e.SEmp, "public_TEmployeeInfo_INDEX_VV_1201_1200");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "public_TEmployeeInfo_INDEX_VV_126832_126831");

            entity.HasIndex(e => e.Code, "public_TEmployeeInfo_INDEX_VV_1280_1279");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "public_TEmployeeInfo_INDEX_VV_66390_66389");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "public_TEmployeeInfo_INDEX_VV_94247_94246");

            entity.HasIndex(e => new { e.Id, e.SchoolId }, "public_TEmployeeInfo_INDEX_VV_98004_98003");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BloodType).HasMaxLength(5);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Ethnicity).HasMaxLength(50);
            entity.Property(e => e.FirstNameEn).HasMaxLength(256);
            entity.Property(e => e.LastNameEn).HasMaxLength(256);
            entity.Property(e => e.Nationality).HasMaxLength(50);
            entity.Property(e => e.PassportCountry).HasMaxLength(100);
            entity.Property(e => e.PassportExpirationDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.PassportNumber).HasMaxLength(50);
            entity.Property(e => e.Religion).HasMaxLength(50);
            entity.Property(e => e.SpouseFirstName).HasMaxLength(30);
            entity.Property(e => e.SpouseLastName).HasMaxLength(50);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.VisaExpirationDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.VisaNo).HasMaxLength(50);
            entity.Property(e => e.WorkPermitExpirationDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.WorkPermitNo).HasMaxLength(50);
        });

        modelBuilder.Entity<TemployeeType>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTypeId }).HasName("public_TEmployeeType_PK_TEmployeeType");

            entity.ToTable("TEmployeeType");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTypeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nTypeId");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.MasterCode).HasMaxLength(20);
            entity.Property(e => e.NTypeId2).HasColumnName("nTypeId2");
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TfamilyProfile>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NFamilyId }).HasName("public_TFamilyProfile_PK_TFamilyProfile");

            entity.ToTable("TFamilyProfile");

            entity.HasIndex(e => e.NFamilyId, "public_TFamilyProfile_IDX_M_580_579");

            entity.HasIndex(e => e.CDel, "public_TFamilyProfile_INDEX_VV_714_713");

            entity.HasIndex(e => new { e.SId, e.SchoolId }, "public_TFamilyProfile_INDEX_VV_8468_8467");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "public_TFamilyProfile_IX_TFamilyProfile_Student");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NFamilyId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nFamilyID");
            entity.Property(e => e.BornFrom)
                .HasMaxLength(100)
                .HasColumnName("bornFrom");
            entity.Property(e => e.BornFromAumpher).HasColumnName("bornFromAumpher");
            entity.Property(e => e.BornFromEn)
                .HasMaxLength(100)
                .HasColumnName("bornFromEn");
            entity.Property(e => e.BornFromProvince).HasColumnName("bornFromProvince");
            entity.Property(e => e.BornFromTumbon).HasColumnName("bornFromTumbon");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DFamilyBirthDay)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dFamilyBirthDay");
            entity.Property(e => e.DFatherBirthDay)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dFatherBirthDay");
            entity.Property(e => e.DMotherBirthDay)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dMotherBirthDay");
            entity.Property(e => e.FamilyStatus).HasColumnName("familyStatus");
            entity.Property(e => e.FriendLastName)
                .HasMaxLength(50)
                .HasColumnName("friendLastName");
            entity.Property(e => e.FriendName)
                .HasMaxLength(50)
                .HasColumnName("friendName");
            entity.Property(e => e.FriendPhone)
                .HasMaxLength(30)
                .HasColumnName("friendPhone");
            entity.Property(e => e.FriendSid).HasColumnName("friendSID");
            entity.Property(e => e.FriendSubLevel).HasColumnName("friendSubLevel");
            entity.Property(e => e.HouseRegistrationAumpher).HasColumnName("houseRegistrationAumpher");
            entity.Property(e => e.HouseRegistrationMuu)
                .HasMaxLength(100)
                .HasColumnName("houseRegistrationMuu");
            entity.Property(e => e.HouseRegistrationNumber)
                .HasMaxLength(100)
                .HasColumnName("houseRegistrationNumber");
            entity.Property(e => e.HouseRegistrationPhone)
                .HasMaxLength(20)
                .HasColumnName("houseRegistrationPhone");
            entity.Property(e => e.HouseRegistrationPost)
                .HasMaxLength(20)
                .HasColumnName("houseRegistrationPost");
            entity.Property(e => e.HouseRegistrationProvince).HasColumnName("houseRegistrationProvince");
            entity.Property(e => e.HouseRegistrationRoad)
                .HasMaxLength(100)
                .HasColumnName("houseRegistrationRoad");
            entity.Property(e => e.HouseRegistrationSoy)
                .HasMaxLength(100)
                .HasColumnName("houseRegistrationSoy");
            entity.Property(e => e.HouseRegistrationTumbon).HasColumnName("houseRegistrationTumbon");
            entity.Property(e => e.NFamilyIncome).HasColumnName("nFamilyIncome");
            entity.Property(e => e.NFamilyRequestStudyMoney).HasColumnName("nFamilyRequestStudyMoney");
            entity.Property(e => e.NFatherIncome).HasColumnName("nFatherIncome");
            entity.Property(e => e.NMotherIncome).HasColumnName("nMotherIncome");
            entity.Property(e => e.NRelativeStudyHere).HasColumnName("nRelativeStudyHere");
            entity.Property(e => e.NSonTotal).HasColumnName("nSonTotal");
            entity.Property(e => e.SDeleted)
                .HasMaxLength(10)
                .HasColumnName("sDeleted");
            entity.Property(e => e.SFamilyAumpher)
                .HasMaxLength(50)
                .HasColumnName("sFamilyAumpher");
            entity.Property(e => e.SFamilyGraduated).HasColumnName("sFamilyGraduated");
            entity.Property(e => e.SFamilyHomeNumber)
                .HasMaxLength(50)
                .HasColumnName("sFamilyHomeNumber");
            entity.Property(e => e.SFamilyIdCardNumber)
                .HasMaxLength(20)
                .HasColumnName("sFamilyIdCardNumber");
            entity.Property(e => e.SFamilyJob)
                .HasMaxLength(200)
                .HasColumnName("sFamilyJob");
            entity.Property(e => e.SFamilyLast)
                .HasMaxLength(50)
                .HasColumnName("sFamilyLast");
            entity.Property(e => e.SFamilyLastEn)
                .HasMaxLength(50)
                .HasColumnName("sFamilyLastEN");
            entity.Property(e => e.SFamilyMuu)
                .HasMaxLength(50)
                .HasColumnName("sFamilyMuu");
            entity.Property(e => e.SFamilyName)
                .HasMaxLength(50)
                .HasColumnName("sFamilyName");
            entity.Property(e => e.SFamilyNameEn)
                .HasMaxLength(50)
                .HasColumnName("sFamilyNameEN");
            entity.Property(e => e.SFamilyNation)
                .HasMaxLength(50)
                .HasColumnName("sFamilyNation");
            entity.Property(e => e.SFamilyPost)
                .HasMaxLength(50)
                .HasColumnName("sFamilyPost");
            entity.Property(e => e.SFamilyProvince)
                .HasMaxLength(50)
                .HasColumnName("sFamilyProvince");
            entity.Property(e => e.SFamilyRace)
                .HasMaxLength(50)
                .HasColumnName("sFamilyRace");
            entity.Property(e => e.SFamilyRelate)
                .HasMaxLength(20)
                .HasColumnName("sFamilyRelate");
            entity.Property(e => e.SFamilyReligion)
                .HasMaxLength(50)
                .HasColumnName("sFamilyReligion");
            entity.Property(e => e.SFamilyRoad)
                .HasMaxLength(50)
                .HasColumnName("sFamilyRoad");
            entity.Property(e => e.SFamilySoy)
                .HasMaxLength(50)
                .HasColumnName("sFamilySoy");
            entity.Property(e => e.SFamilyTitle)
                .HasMaxLength(50)
                .HasColumnName("sFamilyTitle");
            entity.Property(e => e.SFamilyTumbon)
                .HasMaxLength(50)
                .HasColumnName("sFamilyTumbon");
            entity.Property(e => e.SFamilyWorkPlace)
                .HasMaxLength(200)
                .HasColumnName("sFamilyWorkPlace");
            entity.Property(e => e.SFatherAumpher)
                .HasMaxLength(50)
                .HasColumnName("sFatherAumpher");
            entity.Property(e => e.SFatherFirstName)
                .HasMaxLength(50)
                .HasColumnName("sFatherFirstName");
            entity.Property(e => e.SFatherGraduated).HasColumnName("sFatherGraduated");
            entity.Property(e => e.SFatherHomeNumber)
                .HasMaxLength(50)
                .HasColumnName("sFatherHomeNumber");
            entity.Property(e => e.SFatherIdCardNumber)
                .HasMaxLength(50)
                .HasColumnName("sFatherIdCardNumber");
            entity.Property(e => e.SFatherJob)
                .HasMaxLength(100)
                .HasColumnName("sFatherJob");
            entity.Property(e => e.SFatherLastEn)
                .HasMaxLength(100)
                .HasColumnName("sFatherLastEN");
            entity.Property(e => e.SFatherLastName)
                .HasMaxLength(50)
                .HasColumnName("sFatherLastName");
            entity.Property(e => e.SFatherMuu)
                .HasMaxLength(50)
                .HasColumnName("sFatherMuu");
            entity.Property(e => e.SFatherNameEn)
                .HasMaxLength(100)
                .HasColumnName("sFatherNameEN");
            entity.Property(e => e.SFatherNation)
                .HasMaxLength(50)
                .HasColumnName("sFatherNation");
            entity.Property(e => e.SFatherPhone)
                .HasMaxLength(50)
                .HasColumnName("sFatherPhone");
            entity.Property(e => e.SFatherPhone2)
                .HasMaxLength(20)
                .HasColumnName("sFatherPhone2");
            entity.Property(e => e.SFatherPhone3)
                .HasMaxLength(20)
                .HasColumnName("sFatherPhone3");
            entity.Property(e => e.SFatherPost)
                .HasMaxLength(50)
                .HasColumnName("sFatherPost");
            entity.Property(e => e.SFatherProvince)
                .HasMaxLength(50)
                .HasColumnName("sFatherProvince");
            entity.Property(e => e.SFatherRace)
                .HasMaxLength(50)
                .HasColumnName("sFatherRace");
            entity.Property(e => e.SFatherReligion)
                .HasMaxLength(50)
                .HasColumnName("sFatherReligion");
            entity.Property(e => e.SFatherRoad)
                .HasMaxLength(50)
                .HasColumnName("sFatherRoad");
            entity.Property(e => e.SFatherSoy)
                .HasMaxLength(50)
                .HasColumnName("sFatherSoy");
            entity.Property(e => e.SFatherTitle)
                .HasMaxLength(50)
                .HasColumnName("sFatherTitle");
            entity.Property(e => e.SFatherTumbon)
                .HasMaxLength(50)
                .HasColumnName("sFatherTumbon");
            entity.Property(e => e.SFatherWorkPlace)
                .HasMaxLength(200)
                .HasColumnName("sFatherWorkPlace");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SMotherAumpher)
                .HasMaxLength(50)
                .HasColumnName("sMotherAumpher");
            entity.Property(e => e.SMotherFirstName)
                .HasMaxLength(50)
                .HasColumnName("sMotherFirstName");
            entity.Property(e => e.SMotherGraduated).HasColumnName("sMotherGraduated");
            entity.Property(e => e.SMotherHomeNumber)
                .HasMaxLength(50)
                .HasColumnName("sMotherHomeNumber");
            entity.Property(e => e.SMotherIdCardNumber)
                .HasMaxLength(50)
                .HasColumnName("sMotherIdCardNumber");
            entity.Property(e => e.SMotherJob)
                .HasMaxLength(100)
                .HasColumnName("sMotherJob");
            entity.Property(e => e.SMotherLastEn)
                .HasMaxLength(100)
                .HasColumnName("sMotherLastEN");
            entity.Property(e => e.SMotherLastName)
                .HasMaxLength(50)
                .HasColumnName("sMotherLastName");
            entity.Property(e => e.SMotherMuu)
                .HasMaxLength(50)
                .HasColumnName("sMotherMuu");
            entity.Property(e => e.SMotherNameEn)
                .HasMaxLength(100)
                .HasColumnName("sMotherNameEN");
            entity.Property(e => e.SMotherNation)
                .HasMaxLength(50)
                .HasColumnName("sMotherNation");
            entity.Property(e => e.SMotherPhone)
                .HasMaxLength(50)
                .HasColumnName("sMotherPhone");
            entity.Property(e => e.SMotherPhone2)
                .HasMaxLength(20)
                .HasColumnName("sMotherPhone2");
            entity.Property(e => e.SMotherPhone3)
                .HasMaxLength(20)
                .HasColumnName("sMotherPhone3");
            entity.Property(e => e.SMotherPost)
                .HasMaxLength(50)
                .HasColumnName("sMotherPost");
            entity.Property(e => e.SMotherProvince)
                .HasMaxLength(50)
                .HasColumnName("sMotherProvince");
            entity.Property(e => e.SMotherRace)
                .HasMaxLength(50)
                .HasColumnName("sMotherRace");
            entity.Property(e => e.SMotherReligion)
                .HasMaxLength(50)
                .HasColumnName("sMotherReligion");
            entity.Property(e => e.SMotherRoad)
                .HasMaxLength(50)
                .HasColumnName("sMotherRoad");
            entity.Property(e => e.SMotherSoy)
                .HasMaxLength(50)
                .HasColumnName("sMotherSoy");
            entity.Property(e => e.SMotherTitle)
                .HasMaxLength(50)
                .HasColumnName("sMotherTitle");
            entity.Property(e => e.SMotherTumbon)
                .HasMaxLength(50)
                .HasColumnName("sMotherTumbon");
            entity.Property(e => e.SMotherWorkPlace)
                .HasMaxLength(200)
                .HasColumnName("sMotherWorkPlace");
            entity.Property(e => e.SPhoneMail)
                .HasMaxLength(50)
                .HasColumnName("sPhoneMail");
            entity.Property(e => e.SPhoneOne)
                .HasMaxLength(30)
                .HasColumnName("sPhoneOne");
            entity.Property(e => e.SPhoneThree)
                .HasMaxLength(30)
                .HasColumnName("sPhoneThree");
            entity.Property(e => e.SPhoneTwo)
                .HasMaxLength(30)
                .HasColumnName("sPhoneTwo");
            entity.Property(e => e.StayWithEmail)
                .HasMaxLength(100)
                .HasColumnName("stayWithEmail");
            entity.Property(e => e.StayWithEmergencyCall)
                .HasMaxLength(20)
                .HasColumnName("stayWithEmergencyCall");
            entity.Property(e => e.StayWithLast)
                .HasMaxLength(100)
                .HasColumnName("stayWithLast");
            entity.Property(e => e.StayWithName)
                .HasMaxLength(100)
                .HasColumnName("stayWithName");
            entity.Property(e => e.StayWithTitle).HasColumnName("stayWithTitle");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TfeatureSbplu>(entity =>
        {
            entity.HasKey(e => e.FeatureId).HasName("TFeatureSBPlus_pkey");

            entity.ToTable("TFeatureSBPlus");

            entity.HasIndex(e => e.SchoolId, "TFeatureSBPlus_schoolID_idx");

            entity.Property(e => e.FeatureId).HasColumnName("feature_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.DefaultIcon)
                .HasMaxLength(20)
                .HasColumnName("default_icon");
            entity.Property(e => e.DefaultTitle)
                .HasMaxLength(150)
                .HasColumnName("default_title");
            entity.Property(e => e.FeatureCode).HasColumnType("character varying");
            entity.Property(e => e.FeatureDescription).HasColumnName("feature_description");
            entity.Property(e => e.SchoolId)
                .HasMaxLength(50)
                .HasColumnName("schoolID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<TgradeAttendance>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NGradeAttendanceId }).HasName("public_TGradeAttendance_PK_TGradeAttendance");

            entity.ToTable("TGradeAttendance");

            entity.HasIndex(e => new { e.NGradeId, e.SId }, "public_TGradeAttendance_IX_TGradeAttendance");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NGradeAttendanceId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nGradeAttendanceId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.Totalcome)
                .HasMaxLength(3)
                .HasColumnName("totalcome");
            entity.Property(e => e.Totalleave)
                .HasMaxLength(3)
                .HasColumnName("totalleave");
            entity.Property(e => e.Totalsick)
                .HasMaxLength(3)
                .HasColumnName("totalsick");
            entity.Property(e => e.Totalskip)
                .HasMaxLength(3)
                .HasColumnName("totalskip");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Week101)
                .HasMaxLength(1)
                .HasColumnName("week10_1");
            entity.Property(e => e.Week102)
                .HasMaxLength(1)
                .HasColumnName("week10_2");
            entity.Property(e => e.Week103)
                .HasMaxLength(1)
                .HasColumnName("week10_3");
            entity.Property(e => e.Week104)
                .HasMaxLength(1)
                .HasColumnName("week10_4");
            entity.Property(e => e.Week105)
                .HasMaxLength(1)
                .HasColumnName("week10_5");
            entity.Property(e => e.Week106)
                .HasMaxLength(1)
                .HasColumnName("week10_6");
            entity.Property(e => e.Week107)
                .HasMaxLength(1)
                .HasColumnName("week10_7");
            entity.Property(e => e.Week11)
                .HasMaxLength(1)
                .HasColumnName("week1_1");
            entity.Property(e => e.Week111)
                .HasMaxLength(1)
                .HasColumnName("week11_1");
            entity.Property(e => e.Week112)
                .HasMaxLength(1)
                .HasColumnName("week11_2");
            entity.Property(e => e.Week113)
                .HasMaxLength(1)
                .HasColumnName("week11_3");
            entity.Property(e => e.Week114)
                .HasMaxLength(1)
                .HasColumnName("week11_4");
            entity.Property(e => e.Week115)
                .HasMaxLength(1)
                .HasColumnName("week11_5");
            entity.Property(e => e.Week116)
                .HasMaxLength(1)
                .HasColumnName("week11_6");
            entity.Property(e => e.Week117)
                .HasMaxLength(1)
                .HasColumnName("week11_7");
            entity.Property(e => e.Week12)
                .HasMaxLength(1)
                .HasColumnName("week1_2");
            entity.Property(e => e.Week121)
                .HasMaxLength(1)
                .HasColumnName("week12_1");
            entity.Property(e => e.Week122)
                .HasMaxLength(1)
                .HasColumnName("week12_2");
            entity.Property(e => e.Week123)
                .HasMaxLength(1)
                .HasColumnName("week12_3");
            entity.Property(e => e.Week124)
                .HasMaxLength(1)
                .HasColumnName("week12_4");
            entity.Property(e => e.Week125)
                .HasMaxLength(1)
                .HasColumnName("week12_5");
            entity.Property(e => e.Week126)
                .HasMaxLength(1)
                .HasColumnName("week12_6");
            entity.Property(e => e.Week127)
                .HasMaxLength(1)
                .HasColumnName("week12_7");
            entity.Property(e => e.Week13)
                .HasMaxLength(1)
                .HasColumnName("week1_3");
            entity.Property(e => e.Week131)
                .HasMaxLength(1)
                .HasColumnName("week13_1");
            entity.Property(e => e.Week132)
                .HasMaxLength(1)
                .HasColumnName("week13_2");
            entity.Property(e => e.Week133)
                .HasMaxLength(1)
                .HasColumnName("week13_3");
            entity.Property(e => e.Week134)
                .HasMaxLength(1)
                .HasColumnName("week13_4");
            entity.Property(e => e.Week135)
                .HasMaxLength(1)
                .HasColumnName("week13_5");
            entity.Property(e => e.Week136)
                .HasMaxLength(1)
                .HasColumnName("week13_6");
            entity.Property(e => e.Week137)
                .HasMaxLength(1)
                .HasColumnName("week13_7");
            entity.Property(e => e.Week14)
                .HasMaxLength(1)
                .HasColumnName("week1_4");
            entity.Property(e => e.Week141)
                .HasMaxLength(1)
                .HasColumnName("week14_1");
            entity.Property(e => e.Week142)
                .HasMaxLength(1)
                .HasColumnName("week14_2");
            entity.Property(e => e.Week143)
                .HasMaxLength(1)
                .HasColumnName("week14_3");
            entity.Property(e => e.Week144)
                .HasMaxLength(1)
                .HasColumnName("week14_4");
            entity.Property(e => e.Week145)
                .HasMaxLength(1)
                .HasColumnName("week14_5");
            entity.Property(e => e.Week146)
                .HasMaxLength(1)
                .HasColumnName("week14_6");
            entity.Property(e => e.Week147)
                .HasMaxLength(1)
                .HasColumnName("week14_7");
            entity.Property(e => e.Week15)
                .HasMaxLength(1)
                .HasColumnName("week1_5");
            entity.Property(e => e.Week151)
                .HasMaxLength(1)
                .HasColumnName("week15_1");
            entity.Property(e => e.Week152)
                .HasMaxLength(1)
                .HasColumnName("week15_2");
            entity.Property(e => e.Week153)
                .HasMaxLength(1)
                .HasColumnName("week15_3");
            entity.Property(e => e.Week154)
                .HasMaxLength(1)
                .HasColumnName("week15_4");
            entity.Property(e => e.Week155)
                .HasMaxLength(1)
                .HasColumnName("week15_5");
            entity.Property(e => e.Week156)
                .HasMaxLength(1)
                .HasColumnName("week15_6");
            entity.Property(e => e.Week157)
                .HasMaxLength(1)
                .HasColumnName("week15_7");
            entity.Property(e => e.Week16)
                .HasMaxLength(1)
                .HasColumnName("week1_6");
            entity.Property(e => e.Week161)
                .HasMaxLength(1)
                .HasColumnName("week16_1");
            entity.Property(e => e.Week162)
                .HasMaxLength(1)
                .HasColumnName("week16_2");
            entity.Property(e => e.Week163)
                .HasMaxLength(1)
                .HasColumnName("week16_3");
            entity.Property(e => e.Week164)
                .HasMaxLength(1)
                .HasColumnName("week16_4");
            entity.Property(e => e.Week165)
                .HasMaxLength(1)
                .HasColumnName("week16_5");
            entity.Property(e => e.Week166)
                .HasMaxLength(1)
                .HasColumnName("week16_6");
            entity.Property(e => e.Week167)
                .HasMaxLength(1)
                .HasColumnName("week16_7");
            entity.Property(e => e.Week17)
                .HasMaxLength(1)
                .HasColumnName("week1_7");
            entity.Property(e => e.Week171)
                .HasMaxLength(1)
                .HasColumnName("week17_1");
            entity.Property(e => e.Week172)
                .HasMaxLength(1)
                .HasColumnName("week17_2");
            entity.Property(e => e.Week173)
                .HasMaxLength(1)
                .HasColumnName("week17_3");
            entity.Property(e => e.Week174)
                .HasMaxLength(1)
                .HasColumnName("week17_4");
            entity.Property(e => e.Week175)
                .HasMaxLength(1)
                .HasColumnName("week17_5");
            entity.Property(e => e.Week176)
                .HasMaxLength(1)
                .HasColumnName("week17_6");
            entity.Property(e => e.Week177)
                .HasMaxLength(1)
                .HasColumnName("week17_7");
            entity.Property(e => e.Week181)
                .HasMaxLength(1)
                .HasColumnName("week18_1");
            entity.Property(e => e.Week182)
                .HasMaxLength(1)
                .HasColumnName("week18_2");
            entity.Property(e => e.Week183)
                .HasMaxLength(1)
                .HasColumnName("week18_3");
            entity.Property(e => e.Week184)
                .HasMaxLength(1)
                .HasColumnName("week18_4");
            entity.Property(e => e.Week185)
                .HasMaxLength(1)
                .HasColumnName("week18_5");
            entity.Property(e => e.Week186)
                .HasMaxLength(1)
                .HasColumnName("week18_6");
            entity.Property(e => e.Week187)
                .HasMaxLength(1)
                .HasColumnName("week18_7");
            entity.Property(e => e.Week191)
                .HasMaxLength(1)
                .HasColumnName("week19_1");
            entity.Property(e => e.Week192)
                .HasMaxLength(1)
                .HasColumnName("week19_2");
            entity.Property(e => e.Week193)
                .HasMaxLength(1)
                .HasColumnName("week19_3");
            entity.Property(e => e.Week194)
                .HasMaxLength(1)
                .HasColumnName("week19_4");
            entity.Property(e => e.Week195)
                .HasMaxLength(1)
                .HasColumnName("week19_5");
            entity.Property(e => e.Week196)
                .HasMaxLength(1)
                .HasColumnName("week19_6");
            entity.Property(e => e.Week197)
                .HasMaxLength(1)
                .HasColumnName("week19_7");
            entity.Property(e => e.Week201)
                .HasMaxLength(1)
                .HasColumnName("week20_1");
            entity.Property(e => e.Week202)
                .HasMaxLength(1)
                .HasColumnName("week20_2");
            entity.Property(e => e.Week203)
                .HasMaxLength(1)
                .HasColumnName("week20_3");
            entity.Property(e => e.Week204)
                .HasMaxLength(1)
                .HasColumnName("week20_4");
            entity.Property(e => e.Week205)
                .HasMaxLength(1)
                .HasColumnName("week20_5");
            entity.Property(e => e.Week206)
                .HasMaxLength(1)
                .HasColumnName("week20_6");
            entity.Property(e => e.Week207)
                .HasMaxLength(1)
                .HasColumnName("week20_7");
            entity.Property(e => e.Week21)
                .HasMaxLength(1)
                .HasColumnName("week2_1");
            entity.Property(e => e.Week22)
                .HasMaxLength(1)
                .HasColumnName("week2_2");
            entity.Property(e => e.Week23)
                .HasMaxLength(1)
                .HasColumnName("week2_3");
            entity.Property(e => e.Week24)
                .HasMaxLength(1)
                .HasColumnName("week2_4");
            entity.Property(e => e.Week25)
                .HasMaxLength(1)
                .HasColumnName("week2_5");
            entity.Property(e => e.Week26)
                .HasMaxLength(1)
                .HasColumnName("week2_6");
            entity.Property(e => e.Week27)
                .HasMaxLength(1)
                .HasColumnName("week2_7");
            entity.Property(e => e.Week31)
                .HasMaxLength(1)
                .HasColumnName("week3_1");
            entity.Property(e => e.Week32)
                .HasMaxLength(1)
                .HasColumnName("week3_2");
            entity.Property(e => e.Week33)
                .HasMaxLength(1)
                .HasColumnName("week3_3");
            entity.Property(e => e.Week34)
                .HasMaxLength(1)
                .HasColumnName("week3_4");
            entity.Property(e => e.Week35)
                .HasMaxLength(1)
                .HasColumnName("week3_5");
            entity.Property(e => e.Week36)
                .HasMaxLength(1)
                .HasColumnName("week3_6");
            entity.Property(e => e.Week37)
                .HasMaxLength(1)
                .HasColumnName("week3_7");
            entity.Property(e => e.Week41)
                .HasMaxLength(1)
                .HasColumnName("week4_1");
            entity.Property(e => e.Week42)
                .HasMaxLength(1)
                .HasColumnName("week4_2");
            entity.Property(e => e.Week43)
                .HasMaxLength(1)
                .HasColumnName("week4_3");
            entity.Property(e => e.Week44)
                .HasMaxLength(1)
                .HasColumnName("week4_4");
            entity.Property(e => e.Week45)
                .HasMaxLength(1)
                .HasColumnName("week4_5");
            entity.Property(e => e.Week46)
                .HasMaxLength(1)
                .HasColumnName("week4_6");
            entity.Property(e => e.Week47)
                .HasMaxLength(1)
                .HasColumnName("week4_7");
            entity.Property(e => e.Week51)
                .HasMaxLength(1)
                .HasColumnName("week5_1");
            entity.Property(e => e.Week52)
                .HasMaxLength(1)
                .HasColumnName("week5_2");
            entity.Property(e => e.Week53)
                .HasMaxLength(1)
                .HasColumnName("week5_3");
            entity.Property(e => e.Week54)
                .HasMaxLength(1)
                .HasColumnName("week5_4");
            entity.Property(e => e.Week55)
                .HasMaxLength(1)
                .HasColumnName("week5_5");
            entity.Property(e => e.Week56)
                .HasMaxLength(1)
                .HasColumnName("week5_6");
            entity.Property(e => e.Week57)
                .HasMaxLength(1)
                .HasColumnName("week5_7");
            entity.Property(e => e.Week61)
                .HasMaxLength(1)
                .HasColumnName("week6_1");
            entity.Property(e => e.Week62)
                .HasMaxLength(1)
                .HasColumnName("week6_2");
            entity.Property(e => e.Week63)
                .HasMaxLength(1)
                .HasColumnName("week6_3");
            entity.Property(e => e.Week64)
                .HasMaxLength(1)
                .HasColumnName("week6_4");
            entity.Property(e => e.Week65)
                .HasMaxLength(1)
                .HasColumnName("week6_5");
            entity.Property(e => e.Week66)
                .HasMaxLength(1)
                .HasColumnName("week6_6");
            entity.Property(e => e.Week67)
                .HasMaxLength(1)
                .HasColumnName("week6_7");
            entity.Property(e => e.Week71)
                .HasMaxLength(1)
                .HasColumnName("week7_1");
            entity.Property(e => e.Week72)
                .HasMaxLength(1)
                .HasColumnName("week7_2");
            entity.Property(e => e.Week73)
                .HasMaxLength(1)
                .HasColumnName("week7_3");
            entity.Property(e => e.Week74)
                .HasMaxLength(1)
                .HasColumnName("week7_4");
            entity.Property(e => e.Week75)
                .HasMaxLength(1)
                .HasColumnName("week7_5");
            entity.Property(e => e.Week76)
                .HasMaxLength(1)
                .HasColumnName("week7_6");
            entity.Property(e => e.Week77)
                .HasMaxLength(1)
                .HasColumnName("week7_7");
            entity.Property(e => e.Week81)
                .HasMaxLength(1)
                .HasColumnName("week8_1");
            entity.Property(e => e.Week82)
                .HasMaxLength(1)
                .HasColumnName("week8_2");
            entity.Property(e => e.Week83)
                .HasMaxLength(1)
                .HasColumnName("week8_3");
            entity.Property(e => e.Week84)
                .HasMaxLength(1)
                .HasColumnName("week8_4");
            entity.Property(e => e.Week85)
                .HasMaxLength(1)
                .HasColumnName("week8_5");
            entity.Property(e => e.Week86)
                .HasMaxLength(1)
                .HasColumnName("week8_6");
            entity.Property(e => e.Week87)
                .HasMaxLength(1)
                .HasColumnName("week8_7");
            entity.Property(e => e.Week91)
                .HasMaxLength(1)
                .HasColumnName("week9_1");
            entity.Property(e => e.Week92)
                .HasMaxLength(1)
                .HasColumnName("week9_2");
            entity.Property(e => e.Week93)
                .HasMaxLength(1)
                .HasColumnName("week9_3");
            entity.Property(e => e.Week94)
                .HasMaxLength(1)
                .HasColumnName("week9_4");
            entity.Property(e => e.Week95)
                .HasMaxLength(1)
                .HasColumnName("week9_5");
            entity.Property(e => e.Week96)
                .HasMaxLength(1)
                .HasColumnName("week9_6");
            entity.Property(e => e.Week97)
                .HasMaxLength(1)
                .HasColumnName("week9_7");
        });

        modelBuilder.Entity<TgradeCheck>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NGradeCheckId }).HasName("public_TGradeCheck_PK_TGradeCheck");

            entity.ToTable("TGradeCheck");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NGradeCheckId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nGradeCheckId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Deleted).HasMaxLength(10);
            entity.Property(e => e.TeacherId).HasColumnName("teacherId");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TgradeLock>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NGradeLock }).HasName("public_TGradeLock_PK_TGradeLock");

            entity.ToTable("TGradeLock");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NGradeLock)
                .ValueGeneratedOnAdd()
                .HasColumnName("nGradeLock");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("lastUpdate");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.ScoreBehavior1).HasColumnName("scoreBehavior1");
            entity.Property(e => e.ScoreBehavior10).HasColumnName("scoreBehavior10");
            entity.Property(e => e.ScoreBehavior2).HasColumnName("scoreBehavior2");
            entity.Property(e => e.ScoreBehavior3).HasColumnName("scoreBehavior3");
            entity.Property(e => e.ScoreBehavior4).HasColumnName("scoreBehavior4");
            entity.Property(e => e.ScoreBehavior5).HasColumnName("scoreBehavior5");
            entity.Property(e => e.ScoreBehavior6).HasColumnName("scoreBehavior6");
            entity.Property(e => e.ScoreBehavior7).HasColumnName("scoreBehavior7");
            entity.Property(e => e.ScoreBehavior8).HasColumnName("scoreBehavior8");
            entity.Property(e => e.ScoreBehavior9).HasColumnName("scoreBehavior9");
            entity.Property(e => e.ScoreCheewat1).HasColumnName("scoreCheewat1");
            entity.Property(e => e.ScoreCheewat10).HasColumnName("scoreCheewat10");
            entity.Property(e => e.ScoreCheewat11).HasColumnName("scoreCheewat11");
            entity.Property(e => e.ScoreCheewat12).HasColumnName("scoreCheewat12");
            entity.Property(e => e.ScoreCheewat13).HasColumnName("scoreCheewat13");
            entity.Property(e => e.ScoreCheewat14).HasColumnName("scoreCheewat14");
            entity.Property(e => e.ScoreCheewat15).HasColumnName("scoreCheewat15");
            entity.Property(e => e.ScoreCheewat16).HasColumnName("scoreCheewat16");
            entity.Property(e => e.ScoreCheewat17).HasColumnName("scoreCheewat17");
            entity.Property(e => e.ScoreCheewat18).HasColumnName("scoreCheewat18");
            entity.Property(e => e.ScoreCheewat19).HasColumnName("scoreCheewat19");
            entity.Property(e => e.ScoreCheewat2).HasColumnName("scoreCheewat2");
            entity.Property(e => e.ScoreCheewat20).HasColumnName("scoreCheewat20");
            entity.Property(e => e.ScoreCheewat3).HasColumnName("scoreCheewat3");
            entity.Property(e => e.ScoreCheewat4).HasColumnName("scoreCheewat4");
            entity.Property(e => e.ScoreCheewat5).HasColumnName("scoreCheewat5");
            entity.Property(e => e.ScoreCheewat6).HasColumnName("scoreCheewat6");
            entity.Property(e => e.ScoreCheewat7).HasColumnName("scoreCheewat7");
            entity.Property(e => e.ScoreCheewat8).HasColumnName("scoreCheewat8");
            entity.Property(e => e.ScoreCheewat9).HasColumnName("scoreCheewat9");
            entity.Property(e => e.ScoreFinal1).HasColumnName("scoreFinal1");
            entity.Property(e => e.ScoreFinal10).HasColumnName("scoreFinal10");
            entity.Property(e => e.ScoreFinal2).HasColumnName("scoreFinal2");
            entity.Property(e => e.ScoreFinal3).HasColumnName("scoreFinal3");
            entity.Property(e => e.ScoreFinal4).HasColumnName("scoreFinal4");
            entity.Property(e => e.ScoreFinal5).HasColumnName("scoreFinal5");
            entity.Property(e => e.ScoreFinal6).HasColumnName("scoreFinal6");
            entity.Property(e => e.ScoreFinal7).HasColumnName("scoreFinal7");
            entity.Property(e => e.ScoreFinal8).HasColumnName("scoreFinal8");
            entity.Property(e => e.ScoreFinal9).HasColumnName("scoreFinal9");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreGrade1).HasColumnName("scoreGrade1");
            entity.Property(e => e.ScoreGrade10).HasColumnName("scoreGrade10");
            entity.Property(e => e.ScoreGrade11).HasColumnName("scoreGrade11");
            entity.Property(e => e.ScoreGrade12).HasColumnName("scoreGrade12");
            entity.Property(e => e.ScoreGrade13).HasColumnName("scoreGrade13");
            entity.Property(e => e.ScoreGrade14).HasColumnName("scoreGrade14");
            entity.Property(e => e.ScoreGrade15).HasColumnName("scoreGrade15");
            entity.Property(e => e.ScoreGrade16).HasColumnName("scoreGrade16");
            entity.Property(e => e.ScoreGrade17).HasColumnName("scoreGrade17");
            entity.Property(e => e.ScoreGrade18).HasColumnName("scoreGrade18");
            entity.Property(e => e.ScoreGrade19).HasColumnName("scoreGrade19");
            entity.Property(e => e.ScoreGrade2).HasColumnName("scoreGrade2");
            entity.Property(e => e.ScoreGrade20).HasColumnName("scoreGrade20");
            entity.Property(e => e.ScoreGrade3).HasColumnName("scoreGrade3");
            entity.Property(e => e.ScoreGrade4).HasColumnName("scoreGrade4");
            entity.Property(e => e.ScoreGrade5).HasColumnName("scoreGrade5");
            entity.Property(e => e.ScoreGrade6).HasColumnName("scoreGrade6");
            entity.Property(e => e.ScoreGrade7).HasColumnName("scoreGrade7");
            entity.Property(e => e.ScoreGrade8).HasColumnName("scoreGrade8");
            entity.Property(e => e.ScoreGrade9).HasColumnName("scoreGrade9");
            entity.Property(e => e.ScoreMid1).HasColumnName("scoreMid1");
            entity.Property(e => e.ScoreMid10).HasColumnName("scoreMid10");
            entity.Property(e => e.ScoreMid2).HasColumnName("scoreMid2");
            entity.Property(e => e.ScoreMid3).HasColumnName("scoreMid3");
            entity.Property(e => e.ScoreMid4).HasColumnName("scoreMid4");
            entity.Property(e => e.ScoreMid5).HasColumnName("scoreMid5");
            entity.Property(e => e.ScoreMid6).HasColumnName("scoreMid6");
            entity.Property(e => e.ScoreMid7).HasColumnName("scoreMid7");
            entity.Property(e => e.ScoreMid8).HasColumnName("scoreMid8");
            entity.Property(e => e.ScoreMid9).HasColumnName("scoreMid9");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdateByEmp).HasColumnName("updateByEMP");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TgradeMigration>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.GradeMigrationId }).HasName("public_TGradeMigration_PK_TGradeMigration");

            entity.ToTable("TGradeMigration");

            entity.HasIndex(e => new { e.AssessmentId, e.SchoolId }, "public_TGradeMigration_INDEX_VV_1038_1037");

            entity.HasIndex(e => new { e.NGradeId, e.SchoolId }, "public_TGradeMigration_INDEX_VV_3642_3641");

            entity.HasIndex(e => new { e.NGradeId, e.NTermSubLevel2, e.SchoolId, e.ApplicationName, e.ExamId }, "public_TGradeMigration_INDEX_VV_67778_67777");

            entity.HasIndex(e => new { e.SId, e.AssessmentId, e.ExamId, e.NTermSubLevel2, e.SchoolId }, "public_TGradeMigration_INDEX_VV_89364_89363");

            entity.HasIndex(e => new { e.NTermSubLevel2, e.SchoolId, e.CDel }, "public_TGradeMigration_INDEX_VV_94813_94812");

            entity.HasIndex(e => e.ExamId, "public_TGradeMigration_IX_TGradeMigration_nTermSubLevel2_School");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.GradeMigrationId).ValueGeneratedOnAdd();
            entity.Property(e => e.ApplicationName).HasMaxLength(30);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TgradeRegisterPeriod>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NGradeRegisterPeriod }).HasName("public_TGradeRegisterPeriod_PK_TGradeRegisterPeriod");

            entity.ToTable("TGradeRegisterPeriod");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NGradeRegisterPeriod)
                .ValueGeneratedOnAdd()
                .HasColumnName("nGradeRegisterPeriod");
            entity.Property(e => e.AfterMidtermEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("afterMidtermEnd");
            entity.Property(e => e.AfterMidtermStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("afterMidtermStart");
            entity.Property(e => e.BeforeMidtermEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("beforeMidtermEnd");
            entity.Property(e => e.BeforeMidtermStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("beforeMidtermStart");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DuringMidtermEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("duringMidtermEnd");
            entity.Property(e => e.DuringMidtermStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("duringMidtermStart");
            entity.Property(e => e.ExtraEnd).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ExtraStart).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FinaltermEnd).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FinaltermStart).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("lastUpdate");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.UpdateByEmp).HasColumnName("updateByEmp");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TgradeShareInfo>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NGradeShareInfoId }).HasName("public_TGradeShareInfo_PK_TGradeShareInfo");

            entity.ToTable("TGradeShareInfo");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NGradeShareInfoId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nGradeShareInfoId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FromNTsubLevel2).HasColumnName("from_nTSubLevel2");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.ToNTsubLevel2).HasColumnName("to_nTSubLevel2");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TgradeTeacherDescribe>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NGradeTeacherDescribe }).HasName("public_TGradeTeacherDescribe_PK_TGradeTeacherDescribe");

            entity.ToTable("TGradeTeacherDescribe");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NGradeTeacherDescribe)
                .ValueGeneratedOnAdd()
                .HasColumnName("nGradeTeacherDescribe");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Describe).HasMaxLength(500);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TgradeViewRoomListSetting>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.RoomListSettingId }).HasName("public_TGradeViewRoomListSetting_PK_TGradeViewRoomListSetting");

            entity.ToTable("TGradeViewRoomListSetting");

            entity.HasIndex(e => new { e.IsRoomBlocked, e.GradeViewSettingId, e.NTermSubLevel2 }, "public_TGradeViewRoomListSetting_INDEX_VV_5206679_5206678");

            entity.HasIndex(e => new { e.GradeViewSettingId, e.NTermSubLevel2 }, "public_TGradeViewRoomListSetting_INDEX_VV_94846_94845");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.RoomListSettingId).ValueGeneratedOnAdd();
            entity.Property(e => e.ApprovedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TgradeViewSetting>(entity =>
        {
            entity.HasKey(e => e.GradeViewSettingId).HasName("public_TGradeViewSetting_PK_TGradeViewSetting");

            entity.ToTable("TGradeViewSetting");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.ApprovedBy, e.IsTermApproved }, "public_TGradeViewSetting_IX_TGradeViewSetting");

            entity.Property(e => e.ApprovedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TgradeViewStudentBlockListSetting>(entity =>
        {
            entity.HasKey(e => e.StudentBlockListSettingId).HasName("public_TGradeViewStudentBlockListSetting_PK_TGradeViewStudentBl");

            entity.ToTable("TGradeViewStudentBlockListSetting");

            entity.HasIndex(e => new { e.GradeViewSettingId, e.SchoolId }, "public_TGradeViewStudentBlockListSetting_INDEX_VV_33943_33942");

            entity.HasIndex(e => e.SId, "public_TGradeViewStudentBlockListSetting_IX_TGradeViewStudentBl");

            entity.Property(e => e.BlockedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<ThealtProfile>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NHealthId }).HasName("public_THealtProfile_PK_THealtProfile");

            entity.ToTable("THealtProfile");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NHealthId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nHealthID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Height11)
                .HasMaxLength(10)
                .HasColumnName("Height1_1");
            entity.Property(e => e.Height12)
                .HasMaxLength(10)
                .HasColumnName("Height1_2");
            entity.Property(e => e.Height13)
                .HasMaxLength(10)
                .HasColumnName("Height1_3");
            entity.Property(e => e.Height14)
                .HasMaxLength(10)
                .HasColumnName("Height1_4");
            entity.Property(e => e.Height21)
                .HasMaxLength(10)
                .HasColumnName("Height2_1");
            entity.Property(e => e.Height22)
                .HasMaxLength(10)
                .HasColumnName("Height2_2");
            entity.Property(e => e.Height23)
                .HasMaxLength(10)
                .HasColumnName("Height2_3");
            entity.Property(e => e.Height24)
                .HasMaxLength(10)
                .HasColumnName("Height2_4");
            entity.Property(e => e.Height31)
                .HasMaxLength(10)
                .HasColumnName("Height3_1");
            entity.Property(e => e.Height32)
                .HasMaxLength(10)
                .HasColumnName("Height3_2");
            entity.Property(e => e.Height33)
                .HasMaxLength(10)
                .HasColumnName("Height3_3");
            entity.Property(e => e.Height34)
                .HasMaxLength(10)
                .HasColumnName("Height3_4");
            entity.Property(e => e.Height41)
                .HasMaxLength(10)
                .HasColumnName("Height4_1");
            entity.Property(e => e.Height42)
                .HasMaxLength(10)
                .HasColumnName("Height4_2");
            entity.Property(e => e.Height43)
                .HasMaxLength(10)
                .HasColumnName("Height4_3");
            entity.Property(e => e.Height44)
                .HasMaxLength(10)
                .HasColumnName("Height4_4");
            entity.Property(e => e.Height51)
                .HasMaxLength(10)
                .HasColumnName("Height5_1");
            entity.Property(e => e.Height52)
                .HasMaxLength(10)
                .HasColumnName("Height5_2");
            entity.Property(e => e.Height53)
                .HasMaxLength(10)
                .HasColumnName("Height5_3");
            entity.Property(e => e.Height54)
                .HasMaxLength(10)
                .HasColumnName("Height5_4");
            entity.Property(e => e.Height61)
                .HasMaxLength(10)
                .HasColumnName("Height6_1");
            entity.Property(e => e.Height62)
                .HasMaxLength(10)
                .HasColumnName("Height6_2");
            entity.Property(e => e.Height63)
                .HasMaxLength(10)
                .HasColumnName("Height6_3");
            entity.Property(e => e.Height64)
                .HasMaxLength(10)
                .HasColumnName("Height6_4");
            entity.Property(e => e.NHeight).HasColumnName("nHeight");
            entity.Property(e => e.NWeight).HasColumnName("nWeight");
            entity.Property(e => e.SBlood)
                .HasMaxLength(255)
                .HasColumnName("sBlood");
            entity.Property(e => e.SDeleted)
                .HasMaxLength(10)
                .HasColumnName("sDeleted");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SSickDanger)
                .HasMaxLength(255)
                .HasColumnName("sSickDanger");
            entity.Property(e => e.SSickDrug)
                .HasMaxLength(255)
                .HasColumnName("sSickDrug");
            entity.Property(e => e.SSickFood)
                .HasMaxLength(255)
                .HasColumnName("sSickFood");
            entity.Property(e => e.SSickNormal)
                .HasMaxLength(255)
                .HasColumnName("sSickNormal");
            entity.Property(e => e.SSickOther)
                .HasMaxLength(255)
                .HasColumnName("sSickOther");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Weight11)
                .HasMaxLength(10)
                .HasColumnName("Weight1_1");
            entity.Property(e => e.Weight12)
                .HasMaxLength(10)
                .HasColumnName("Weight1_2");
            entity.Property(e => e.Weight13)
                .HasMaxLength(10)
                .HasColumnName("Weight1_3");
            entity.Property(e => e.Weight14)
                .HasMaxLength(10)
                .HasColumnName("Weight1_4");
            entity.Property(e => e.Weight21)
                .HasMaxLength(10)
                .HasColumnName("Weight2_1");
            entity.Property(e => e.Weight22)
                .HasMaxLength(10)
                .HasColumnName("Weight2_2");
            entity.Property(e => e.Weight23)
                .HasMaxLength(10)
                .HasColumnName("Weight2_3");
            entity.Property(e => e.Weight24)
                .HasMaxLength(10)
                .HasColumnName("Weight2_4");
            entity.Property(e => e.Weight31)
                .HasMaxLength(10)
                .HasColumnName("Weight3_1");
            entity.Property(e => e.Weight32)
                .HasMaxLength(10)
                .HasColumnName("Weight3_2");
            entity.Property(e => e.Weight33)
                .HasMaxLength(10)
                .HasColumnName("Weight3_3");
            entity.Property(e => e.Weight34)
                .HasMaxLength(10)
                .HasColumnName("Weight3_4");
            entity.Property(e => e.Weight41)
                .HasMaxLength(10)
                .HasColumnName("Weight4_1");
            entity.Property(e => e.Weight42)
                .HasMaxLength(10)
                .HasColumnName("Weight4_2");
            entity.Property(e => e.Weight43)
                .HasMaxLength(10)
                .HasColumnName("Weight4_3");
            entity.Property(e => e.Weight44)
                .HasMaxLength(10)
                .HasColumnName("Weight4_4");
            entity.Property(e => e.Weight51)
                .HasMaxLength(10)
                .HasColumnName("Weight5_1");
            entity.Property(e => e.Weight52)
                .HasMaxLength(10)
                .HasColumnName("Weight5_2");
            entity.Property(e => e.Weight53)
                .HasMaxLength(10)
                .HasColumnName("Weight5_3");
            entity.Property(e => e.Weight54)
                .HasMaxLength(10)
                .HasColumnName("Weight5_4");
            entity.Property(e => e.Weight61)
                .HasMaxLength(10)
                .HasColumnName("Weight6_1");
            entity.Property(e => e.Weight62)
                .HasMaxLength(10)
                .HasColumnName("Weight6_2");
            entity.Property(e => e.Weight63)
                .HasMaxLength(10)
                .HasColumnName("Weight6_3");
            entity.Property(e => e.Weight64)
                .HasMaxLength(10)
                .HasColumnName("Weight6_4");
        });

        modelBuilder.Entity<ThistoryLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_THistoryLog_PK_THistoryLog");

            entity.ToTable("THistoryLog");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Date).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FunctionName).HasMaxLength(100);
            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<Tholiday>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NHoliday }).HasName("public_THoliday_PK_THoliday");

            entity.ToTable("THoliday");

            entity.HasIndex(e => new { e.SHolidayType, e.CDel, e.SWhoSeeThis, e.SchoolId, e.DHolidayStart, e.DHolidayEnd }, "public_THoliday_IDX_M_7685_7684");

            entity.HasIndex(e => new { e.SchoolId, e.DHolidayStart, e.DHolidayEnd }, "public_THoliday_INDEX_VV_100020_100019");

            entity.HasIndex(e => new { e.SHolidayType, e.SchoolId }, "public_THoliday_INDEX_VV_121211_121210");

            entity.HasIndex(e => new { e.CDel, e.SchoolId, e.DHolidayEnd }, "public_THoliday_INDEX_VV_125930_125929");

            entity.HasIndex(e => new { e.SHolidayType, e.CDel, e.SchoolId, e.SWhoSeeThis }, "public_THoliday_INDEX_VV_126908_126907");

            entity.HasIndex(e => new { e.CDel, e.SchoolId }, "public_THoliday_INDEX_VV_20402_20401");

            entity.HasIndex(e => new { e.CDel, e.SWhoSeeThis, e.SchoolId, e.SHolidayType, e.DHolidayStart, e.DHolidayEnd }, "public_THoliday_INDEX_VV_2068_2067");

            entity.HasIndex(e => new { e.CDel, e.SchoolId, e.SHolidayType }, "public_THoliday_INDEX_VV_2919_2918");

            entity.HasIndex(e => new { e.CDel, e.SchoolId, e.SWhoSeeThis }, "public_THoliday_INDEX_VV_84783_84782");

            entity.HasIndex(e => new { e.SchoolId, e.SWhoSeeThis }, "public_THoliday_INDEX_VV_94490_94489");

            entity.HasIndex(e => new { e.CDel, e.SchoolId }, "public_THoliday_INDEX_VV_96813_96812");

            entity.HasIndex(e => new { e.CDel, e.SchoolId, e.DHolidayStart, e.DHolidayEnd }, "public_THoliday_INDEX_VV_98482_98481");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NHoliday)
                .HasMaxLength(10)
                .HasColumnName("nHoliday");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CStatusActive).HasColumnName("cStatusActive");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DHolidayEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dHolidayEnd");
            entity.Property(e => e.DHolidayStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dHolidayStart");
            entity.Property(e => e.SColor).HasColumnName("sColor");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SGroupTarget)
                .HasMaxLength(1)
                .HasColumnName("sGroupTarget");
            entity.Property(e => e.SHoliday)
                .HasMaxLength(250)
                .HasColumnName("sHoliday");
            entity.Property(e => e.SHolidayAll)
                .HasMaxLength(1)
                .HasColumnName("sHolidayAll");
            entity.Property(e => e.SHolidayEn)
                .HasMaxLength(250)
                .HasColumnName("sHolidayEN");
            entity.Property(e => e.SHolidayType)
                .HasMaxLength(1)
                .HasColumnName("sHolidayType");
            entity.Property(e => e.SWhoSeeThis)
                .HasMaxLength(1)
                .HasColumnName("sWhoSeeThis");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TholidayLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("public_THolidayLog_PK_THolidayLog");

            entity.ToTable("THolidayLog");

            entity.HasIndex(e => e.SchoolId, "public_THolidayLog_INDEX_VV_110515_110514");

            entity.HasIndex(e => e.SchoolId, "public_THolidayLog_INDEX_VV_23103_23102");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HolidayId)
                .HasMaxLength(15)
                .HasColumnName("HolidayID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TholidaySome>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NHolidaySomeId }).HasName("public_THolidaySome_PK_THolidaySome");

            entity.ToTable("THolidaySome");

            entity.HasIndex(e => new { e.Deleted, e.SchoolId }, "public_THolidaySome_INDEX_VV_104589_104588");

            entity.HasIndex(e => new { e.Deleted, e.SchoolId }, "public_THolidaySome_INDEX_VV_110603_110602");

            entity.HasIndex(e => new { e.Deleted, e.SchoolId, e.CDel }, "public_THolidaySome_INDEX_VV_126992_126991");

            entity.HasIndex(e => e.NHoliday, "public_THolidaySome_INDEX_VV_6066208_6066207");

            entity.HasIndex(e => e.NHoliday, "public_THolidaySome_IX_THolidaySome_Deleted_SchoolID");

            entity.HasIndex(e => new { e.NTsubLevel, e.SchoolId }, "public_THolidaySome_IX_THolidaySome_nTSubLevel_SchoolID");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NHolidaySomeId)
                .HasMaxLength(50)
                .HasColumnName("nHolidaySomeID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NHoliday)
                .HasMaxLength(50)
                .HasColumnName("nHoliday");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<ThomeWorkFile>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NFileId }).HasName("public_THomeWorkFile_PK_THomeWorkFile");

            entity.ToTable("THomeWorkFile");

            entity.HasIndex(e => e.NHomeWorkId, "public_THomeWorkFile_IX_THomeWorkFile_SchoolID_cDel");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NFileId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nFileId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NHomeWorkId).HasColumnName("nHomeWorkId");
            entity.Property(e => e.SFileName)
                .HasMaxLength(250)
                .HasColumnName("sFileName");
            entity.Property(e => e.Title).HasMaxLength(250);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<ThomeWorkLearning>(entity =>
        {
            entity.HasKey(e => e.LearnId).HasName("public_THomeWorkLearning_PK_THomeWorkLearning");

            entity.ToTable("THomeWorkLearning");

            entity.HasIndex(e => e.SchoolId, "public_THomeWorkLearning_INDEX_VV_24290_24289");

            entity.HasIndex(e => e.OnlineId, "public_THomeWorkLearning_INDEX_VV_5801486_5801485");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DisplayDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LinkYt).HasColumnName("LinkYT");
            entity.Property(e => e.Modified).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SelectedRoom).HasMaxLength(250);
            entity.Property(e => e.SelectedStudent).HasMaxLength(2500);
            entity.Property(e => e.TitleName).HasMaxLength(250);
        });

        modelBuilder.Entity<ThomeWorkLearningFile>(entity =>
        {
            entity.HasKey(e => e.AttachId).HasName("public_THomeWorkLearningFile_PK_THomeWorkLearningFile");

            entity.ToTable("THomeWorkLearningFile");

            entity.HasIndex(e => e.LearnId, "public_THomeWorkLearningFile_INDEX_VV_5118105_5118104");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.SFileName)
                .HasMaxLength(250)
                .HasColumnName("sFileName");
            entity.Property(e => e.Title).HasMaxLength(250);
        });

        modelBuilder.Entity<ThomeWorkReply>(entity =>
        {
            entity.HasKey(e => e.ReplyId).HasName("public_THomeWorkReply_PK_THomeWorkReply");

            entity.ToTable("THomeWorkReply");

            entity.HasIndex(e => e.HomeWorkId, "public_THomeWorkReply_IX_THomeWorkReply_SchoolId");

            entity.HasIndex(e => new { e.Type, e.SchoolId, e.SId }, "public_THomeWorkReply_IX_THomeWorkReply_Type_SchoolId_sID");

            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Modified).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.TId).HasColumnName("tID");
        });

        modelBuilder.Entity<ThomeWorkReplyFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("public_THomeWorkReply_File_PK_THomeWorkReply_File");

            entity.ToTable("THomeWorkReply_File");

            entity.Property(e => e.FileId).HasColumnName("FileID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.CreateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FileTitle).HasMaxLength(250);
            entity.Property(e => e.FileUrl).HasMaxLength(250);
            entity.Property(e => e.HomeWorkId).HasColumnName("HomeWorkID");
            entity.Property(e => e.ReplyId).HasColumnName("ReplyID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<Thomework>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NHomeWork }).HasName("public_THomework_PK_THomework");

            entity.ToTable("THomework");

            entity.HasIndex(e => e.OnlineId, "public_THomework_IDX_M_9443_9442");

            entity.HasIndex(e => e.CDel, "public_THomework_IDX_THomework_nHomeWork");

            entity.HasIndex(e => e.NHomeWork, "public_THomework_INDEX_VV_12231_12230");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.DisplayDate }, "public_THomework_INDEX_VV_126703_126702");

            entity.HasIndex(e => e.OnlineId, "public_THomework_INDEX_VV_5155024_5155023");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.DisplayDate }, "public_THomework_INDEX_VV_89766_89765");

            entity.HasIndex(e => new { e.OnlineId, e.SchoolId, e.CDel, e.DisplayDate }, "public_THomework_IX_THomework_SchoolID_cDel_DisplayDate");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NHomeWork)
                .ValueGeneratedOnAdd()
                .HasColumnName("nHomeWork");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DNotification)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dNotification");
            entity.Property(e => e.DOrder)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dOrder");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.DisplayDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LinkYt).HasColumnName("LinkYT");
            entity.Property(e => e.Modified).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SHomeworkDetail).HasColumnName("sHomeworkDetail");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SelectedRoom).HasMaxLength(250);
            entity.Property(e => e.SelectedStudent).HasMaxLength(2500);
            entity.Property(e => e.TitleName).HasMaxLength(250);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<ThomeworkUser>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NHomeWork, e.SId }).HasName("public_THomework_User_PK_THomework_User");

            entity.ToTable("THomework_User");

            entity.HasIndex(e => new { e.NHomeWork, e.SId }, "public_THomework_User_INDEX_VV_102707_102706");

            entity.HasIndex(e => new { e.SchoolId, e.IsSend, e.CDel }, "public_THomework_User_INDEX_VV_107481_107480");

            entity.HasIndex(e => new { e.SId, e.SchoolId, e.CDel }, "public_THomework_User_INDEX_VV_26401_26400");

            entity.HasIndex(e => new { e.SId, e.SchoolId, e.CreatedDate, e.CDel }, "public_THomework_User_IX_VV_THomework_User_sID_SchoolID_Created");

            entity.HasIndex(e => new { e.SId, e.SchoolId, e.CDel, e.NHomeWork }, "public_THomework_User_IX_VV_THomework_User_sID_SchoolID_cDel_nH");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NHomeWork).HasColumnName("nHomeWork");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.AttachFile).HasMaxLength(250);
            entity.Property(e => e.AttachFileContentType).HasMaxLength(100);
            entity.Property(e => e.AttachTitle).HasMaxLength(250);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CStatusHomeWork)
                .HasMaxLength(1)
                .HasColumnName("cStatusHomeWork");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LinkUrl).HasMaxLength(250);
            entity.Property(e => e.Remark).HasMaxLength(1000);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<ThomeworkUserFile>(entity =>
        {
            entity.HasKey(e => new { e.FileId, e.NHomeWorkId, e.SId, e.SchoolId }).HasName("public_THomework_User_File_PK_THomework_User_File");

            entity.ToTable("THomework_User_File");

            entity.HasIndex(e => new { e.NHomeWorkId, e.SId, e.SchoolId }, "public_THomework_User_File_INDEX_VV_18206_18205");

            entity.Property(e => e.FileId).ValueGeneratedOnAdd();
            entity.Property(e => e.NHomeWorkId).HasColumnName("nHomeWorkId");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.FileContentType).HasMaxLength(100);
            entity.Property(e => e.FileTitle).HasMaxLength(250);
            entity.Property(e => e.FileUrl).HasMaxLength(350);
        });

        modelBuilder.Entity<TjobList>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NJobid }).HasName("public_TJobList_PK_TJobList");

            entity.ToTable("TJobList");

            entity.HasIndex(e => e.NJobid, "public_TJobList_IDX_M_127_126");

            entity.HasIndex(e => new { e.SchoolId, e.Deleted }, "public_TJobList_INDEX_VV_579_578");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "public_TJobList_INDEX_VV_93844_93843");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NJobid)
                .ValueGeneratedOnAdd()
                .HasColumnName("nJobid");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Deleted)
                .HasMaxLength(50)
                .HasColumnName("deleted");
            entity.Property(e => e.EmpType)
                .HasMaxLength(1)
                .HasColumnName("empType");
            entity.Property(e => e.JobDescription)
                .HasMaxLength(100)
                .HasColumnName("jobDescription");
            entity.Property(e => e.NSchoolId).HasColumnName("nSchoolId");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.WorkStatus)
                .HasMaxLength(50)
                .HasColumnName("workStatus");
        });

        modelBuilder.Entity<TlearningCenter>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.LearningCenterId }).HasName("public_TLearningCenter_PK_TLEARNINGCENTER");

            entity.ToTable("TLearningCenter");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.LearningCenterId)
                .ValueGeneratedOnAdd()
                .HasColumnName("LearningCenterID");
            entity.Property(e => e.Admin).HasMaxLength(100);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Detail).HasMaxLength(300);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Tlevel>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.LevelId }).HasName("public_TLevel_PK_TLevel");

            entity.ToTable("TLevel");

            entity.HasIndex(e => new { e.CDel, e.LevelNameEng }, "public_TLevel_INDEX_VV_4019_4018");

            entity.HasIndex(e => new { e.LevelName, e.CDel }, "public_TLevel_INDEX_VV_4317_4316");

            entity.HasIndex(e => e.LevelId, "public_TLevel_INDEX_VV_8638_8637");

            entity.HasIndex(e => e.LevelName, "public_TLevel_IX_TLevel_LevelName");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.LevelId)
                .ValueGeneratedOnAdd()
                .HasColumnName("LevelID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LevelName).HasMaxLength(20);
            entity.Property(e => e.LevelNameEng).HasMaxLength(50);
            entity.Property(e => e.MasterCode).HasMaxLength(2);
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.SortValue).HasColumnName("sortValue");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TmasterDatum>(entity =>
        {
            entity.HasKey(e => e.MasterId).HasName("public_TMasterData_PK_TMasterData");

            entity.ToTable("TMasterData");

            entity.Property(e => e.MasterId).HasColumnName("MasterID");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.MasterCode).HasMaxLength(20);
            entity.Property(e => e.MasterDes).HasMaxLength(250);
            entity.Property(e => e.MasterDesEn).HasMaxLength(250);
            entity.Property(e => e.MasterType).HasMaxLength(20);
        });

        modelBuilder.Entity<TmasterDbuser>(entity =>
        {
            entity.HasKey(e => e.SId).HasName("TMasterDBUser_pkey");

            entity.ToTable("TMasterDBUser");

            entity.HasIndex(e => new { e.NCompany, e.SId, e.CType }, "idx_tmasterdbuser_company_sid_ctype");

            entity.HasIndex(e => e.SId, "idx_tmasterdbuser_sid");

            entity.HasIndex(e => e.SId, "idx_tmasterdbuser_sid_active").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.HasIndex(e => new { e.SId, e.CDel, e.CType, e.NCompany }, "idx_tmasterdbuser_sid_cdel_ctype");

            entity.HasIndex(e => new { e.SId, e.NCompany }, "idx_tmasterdbuser_sid_company").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.HasIndex(e => new { e.SId, e.CType }, "idx_tmasterdbuser_sid_ctype");

            entity.Property(e => e.SId)
                .ValueGeneratedNever()
                .HasColumnName("sID");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasColumnType("character varying")
                .HasColumnName("cType");
            entity.Property(e => e.NCompany).HasColumnName("nCompany");
            entity.Property(e => e.NMoney).HasColumnName("nMoney");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NSystemId).HasColumnName("nSystemID");
            entity.Property(e => e.Nfc)
                .HasColumnType("character varying")
                .HasColumnName("NFC");
            entity.Property(e => e.PasswordHash).HasColumnType("character varying");
            entity.Property(e => e.Pin)
                .HasColumnType("character varying")
                .HasColumnName("pin");
            entity.Property(e => e.SEmail)
                .HasColumnType("character varying")
                .HasColumnName("sEmail");
            entity.Property(e => e.SIdentification)
                .HasColumnType("character varying")
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasColumnType("character varying")
                .HasColumnName("sName");
            entity.Property(e => e.SPicture)
                .HasColumnType("character varying")
                .HasColumnName("sPicture");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(200)
                .HasColumnName("sStudentID");
            entity.Property(e => e.Username)
                .HasColumnType("character varying")
                .HasColumnName("username");
            entity.Property(e => e.Userpassword)
                .HasColumnType("character varying")
                .HasColumnName("userpassword");
        });

        modelBuilder.Entity<TmasterType>(entity =>
        {
            entity.HasKey(e => e.MasterTypeId).HasName("public_TMasterType_PK_TMasterType");

            entity.ToTable("TMasterType");

            entity.Property(e => e.MasterTypeId).HasColumnName("MasterTypeID");
            entity.Property(e => e.MasterTypeNameEn)
                .HasMaxLength(200)
                .HasColumnName("MasterTypeNameEN");
            entity.Property(e => e.MasterTypeNameTh)
                .HasMaxLength(200)
                .HasColumnName("MasterTypeNameTH");
        });

        modelBuilder.Entity<TmessageLinemulticast>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.MessageId, e.GroupId }).HasName("public_TMessageLINEMulticast_PK_TMESSAGELINEMULTICAST");

            entity.ToTable("TMessageLINEMulticast");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.StreamId)
                .HasMaxLength(1500)
                .HasColumnName("StreamID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TnewsFile>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NNewsFileId }).HasName("public_TNewsFiles_PK_TNewsFiles");

            entity.ToTable("TNewsFiles");

            entity.HasIndex(e => new { e.NSms, e.SchoolId, e.CDel }, "public_TNewsFiles_INDEX_VV_51642_51641");

            entity.HasIndex(e => e.NSms, "public_TNewsFiles_IX_TNewsFiles");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NNewsFileId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nNewsFileID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.ContentType).HasMaxLength(200);
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FullFileName).HasMaxLength(200);
            entity.Property(e => e.NSms).HasColumnName("nSMS");
            entity.Property(e => e.SFileName)
                .HasMaxLength(200)
                .HasColumnName("sFileName");
            entity.Property(e => e.StudentCodeFileName).HasMaxLength(20);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Tonetscore>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.Year, e.SId, e.Identification, e.NTsubLevel }).HasName("public_TONETScore_PK_TONETSCORE");

            entity.ToTable("TONETScore");

            entity.HasIndex(e => new { e.SId, e.SchoolId, e.NTsubLevel }, "public_TONETScore_INDEX_VV_115426_115425");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.Identification).HasMaxLength(20);
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Class).HasMaxLength(20);
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Multiplier).HasPrecision(5, 2);
            entity.Property(e => e.Result30Percent).HasPrecision(5, 2);
            entity.Property(e => e.ResultPercent).HasPrecision(5, 2);
            entity.Property(e => e.SeatNo).HasMaxLength(15);
            entity.Property(e => e.SubjectScore01).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore02).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore03).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore04).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore05).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore61).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore63).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore64).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore65).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore91).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore93).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore94).HasPrecision(5, 2);
            entity.Property(e => e.SubjectScore95).HasPrecision(5, 2);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Tparent>(entity =>
        {
            entity.HasKey(e => e.ParentId).HasName("public_TParent_PK_TParent");

            entity.ToTable("TParent");

            entity.HasIndex(e => e.Email, "public_TParent_IX_TParent_Email");

            entity.HasIndex(e => e.Tel, "public_TParent_IX_TParent_Tel");

            entity.HasIndex(e => e.CDel, "public_TParent_IX_TParent_cDel");

            entity.HasIndex(e => e.Email, "public_TParent_UQ_TParent_Email").IsUnique();

            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(27)
                .HasDefaultValueSql("'getdate()'::character varying")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.ParentLastname)
                .HasMaxLength(100)
                .HasColumnName("parent_lastname");
            entity.Property(e => e.ParentName)
                .HasMaxLength(100)
                .HasColumnName("parent_name");
            entity.Property(e => e.ParentTitle)
                .HasMaxLength(50)
                .HasColumnName("parent_title");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Tel)
                .HasMaxLength(20)
                .HasColumnName("tel");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<TparentStudent>(entity =>
        {
            entity.HasKey(e => e.ParentStudentId).HasName("public_TParentStudents_PK_TParentStudents");

            entity.ToTable("TParentStudents");

            entity.HasIndex(e => e.LinkedAt, "public_TParentStudents_IX_TParentStudents_LinkedAt");

            entity.HasIndex(e => e.ParentCode, "public_TParentStudents_IX_TParentStudents_ParentCode");

            entity.HasIndex(e => e.ParentId, "public_TParentStudents_IX_TParentStudents_ParentID");

            entity.HasIndex(e => e.SId, "public_TParentStudents_IX_TParentStudents_SID");

            entity.HasIndex(e => e.SchoolId, "public_TParentStudents_IX_TParentStudents_SchoolID");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "public_TParentStudents_IX_TParentStudents_SchoolID_SID");

            entity.HasIndex(e => e.UnlinkedAt, "public_TParentStudents_IX_TParentStudents_UnlinkedAt");

            entity.HasIndex(e => new { e.ParentId, e.SchoolId, e.SId }, "public_TParentStudents_UQ_TParentStudents_ParentStudent").IsUnique();

            entity.Property(e => e.ParentStudentId).HasColumnName("parent_student_id");
            entity.Property(e => e.LinkedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("linked_at");
            entity.Property(e => e.LinkedBy)
                .HasMaxLength(100)
                .HasColumnName("linked_by");
            entity.Property(e => e.ParentCode)
                .HasMaxLength(13)
                .HasColumnName("parent_code");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Relation)
                .HasMaxLength(20)
                .HasColumnName("relation");
            entity.Property(e => e.RelationOtherText)
                .HasMaxLength(100)
                .HasColumnName("relation_other_text");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UnlinkedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("unlinked_at");
            entity.Property(e => e.UnlinkedBy)
                .HasMaxLength(100)
                .HasColumnName("unlinked_by");

            entity.HasOne(d => d.Parent).WithMany(p => p.TparentStudents)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_TParentStudents_Parent");
        });

        modelBuilder.Entity<Tperiod>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SPeriodId }).HasName("public_TPeriod_PK_TSchedule");

            entity.ToTable("TPeriod");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SPeriodId)
                .HasMaxLength(10)
                .HasColumnName("sPeriodID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.DTimeEndIn)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeEnd_IN");
            entity.Property(e => e.DTimeEndOut)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeEnd_OUT");
            entity.Property(e => e.DTimeStartIn)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeStart_IN");
            entity.Property(e => e.DTimeStartOut)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeStart_OUT");
            entity.Property(e => e.NDay).HasColumnName("nDay");
            entity.Property(e => e.SClassId)
                .HasMaxLength(10)
                .HasColumnName("sClassID");
            entity.Property(e => e.SPeriodName)
                .HasMaxLength(250)
                .HasColumnName("sPeriodName");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SScheduleId)
                .HasMaxLength(10)
                .HasColumnName("sScheduleID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TperiodTsubLevel>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTsubLevel, e.SPeriodId }).HasName("public_TPeriod_TSubLevel_PK_TPeriod_TSubLevel");

            entity.ToTable("TPeriod_TSubLevel");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTsubLevel)
                .ValueGeneratedOnAdd()
                .HasColumnName("nTSubLevel");
            entity.Property(e => e.SPeriodId)
                .HasMaxLength(10)
                .HasColumnName("sPeriodID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TpermissionCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_TPermissionCard_PK_TPermissionCard");

            entity.ToTable("TPermissionCard");

            entity.HasIndex(e => new { e.RefNo, e.SchoolId }, "public_TPermissionCard_INDEX_VV_5661475_5661474");

            entity.HasIndex(e => e.SchoolId, "public_TPermissionCard_INDEX_VV_60407_60406");

            entity.HasIndex(e => e.StudentId, "public_TPermissionCard_INDEX_VV_73545_73544");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AttachUrl).HasMaxLength(500);
            entity.Property(e => e.Cause).HasMaxLength(500);
            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EndDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Modified).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StartDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.TypeId).HasColumnName("TypeID");
        });

        modelBuilder.Entity<TpermissionCardType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_TPermissionCardType_PK_TPermissionType");

            entity.ToTable("TPermissionCardType");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Modified).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Permission).HasMaxLength(250);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<Tplan>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.PlanId }).HasName("public_TPlan_PK_TPlan");

            entity.ToTable("TPlan");

            entity.HasIndex(e => new { e.NTsubLevel, e.IsActive, e.SchoolId }, "public_TPlan_INDEX_VV_102631_102630");

            entity.HasIndex(e => e.PlanId, "public_TPlan_INDEX_VV_263_262");

            entity.HasIndex(e => e.CurriculumId, "public_TPlan_INDEX_VV_5124310_5124309");

            entity.HasIndex(e => e.CurriculumId, "public_TPlan_INDEX_VV_55575_55574");

            entity.HasIndex(e => new { e.PlanName, e.IsActive, e.SchoolId, e.PlanId }, "public_TPlan_IX_TPlan_PlanName_IsActive_SchoolID_PlanId");

            entity.HasIndex(e => new { e.CurriculumId, e.NTsubLevel, e.IsActive, e.SchoolId }, "public_TPlan_ix_TPlan_new");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.PlanId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.PlanName).HasMaxLength(250);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TplanCourse>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.PlanCourseId }).HasName("public_TPlanCourse_PK_TPlanCourse");

            entity.ToTable("TPlanCourse");

            entity.HasIndex(e => new { e.SchoolId, e.SPlaneId }, "idx_plan_course_school");

            entity.HasIndex(e => new { e.SPlaneId, e.PlanId, e.CourseStatus, e.IsActive, e.SchoolId }, "public_TPlanCourse_IDX_M_6856_6855");

            entity.HasIndex(e => e.SPlaneId, "public_TPlanCourse_IDX_M_6859_6858");

            entity.HasIndex(e => new { e.CourseStatus, e.IsActive, e.SchoolId, e.CDel }, "public_TPlanCourse_INDEX_VV_111852_111851");

            entity.HasIndex(e => new { e.PlanId, e.CourseStatus, e.IsActive, e.SchoolId, e.CDel }, "public_TPlanCourse_INDEX_VV_156376_156375");

            entity.HasIndex(e => e.PlanId, "public_TPlanCourse_INDEX_VV_32554_32553");

            entity.HasIndex(e => e.PlanId, "public_TPlanCourse_INDEX_VV_6065602_6065601");

            entity.HasIndex(e => e.PlanCourseId, "public_TPlanCourse_INDEX_VV_64745_64744");

            entity.HasIndex(e => new { e.SchoolId, e.NYear }, "public_TPlanCourse_INDEX_VV_880_879");

            entity.HasIndex(e => new { e.CourseStatus, e.IsActive, e.SchoolId, e.NYear, e.CDel }, "public_TPlanCourse_INDEX_VV_89903_89902");

            entity.HasIndex(e => new { e.CourseStatus, e.IsActive, e.SchoolId }, "public_TPlanCourse_INDEX_VV_90906_90905");

            entity.HasIndex(e => new { e.CourseStatus, e.IsActive, e.SchoolId, e.CDel }, "public_TPlanCourse_INDEX_VV_93421_93420");

            entity.HasIndex(e => new { e.CourseStatus, e.IsActive, e.SchoolId, e.NYear, e.CDel }, "public_TPlanCourse_INDEX_VV_96819_96818");

            entity.HasIndex(e => new { e.PlanId, e.SPlaneId, e.CourseStatus, e.IsActive, e.SchoolId }, "public_TPlanCourse_IX_TPlanCourse_2");

            entity.HasIndex(e => new { e.SchoolId, e.PlanId, e.CourseStatus, e.IsActive }, "public_TPlanCourse_IX_TPlanCourse_Optim");

            entity.HasIndex(e => e.PlanId, "public_TPlanCourse_IX_TPlanCourse_SchoolID_nYear");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.PlanCourseId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NCredit).HasColumnName("nCredit");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.Rb1displayOrder).HasColumnName("RB1DisplayOrder");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TplanCourseAdjustedTerm>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.PlanCourseAdjustedTermId }).HasName("public_TPlanCourseAdjustedTerm_PK_TPlanCourseAdjustedTerm");

            entity.ToTable("TPlanCourseAdjustedTerm");

            entity.HasIndex(e => new { e.PlanCourseId, e.SchoolId, e.NTerm }, "idx_adjusted_term");

            entity.HasIndex(e => new { e.PlanCourseId, e.NTerm }, "public_TPlanCourseAdjustedTerm_IX_TPlanCourseAdjustedTerm");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.PlanCourseAdjustedTermId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TplanCourseAssessmentTopic>(entity =>
        {
            entity.HasKey(e => e.PlanCourseAssessmentTopicId);

            entity.ToTable("TPlanCourseAssessmentTopic");

            entity.Property(e => e.PlanCourseAssessmentTopicId).UseIdentityAlwaysColumn();
            entity.Property(e => e.AssessmentCategoryCode).HasMaxLength(2);
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TplanCourseSection>(entity =>
        {
            entity.HasKey(e => e.PlanCourseSectionId);

            entity.ToTable("TPlanCourseSection");

            entity.Property(e => e.PlanCourseSectionId).UseIdentityAlwaysColumn();
            entity.Property(e => e.CDel)
                .HasDefaultValue(false)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TplanCourseSectionAllowedRoom>(entity =>
        {
            entity.HasKey(e => e.PlanCourseAllowedRoomId).HasName("TPlanCourseSectionAllowedRoom_pkey");

            entity.ToTable("TPlanCourseSectionAllowedRoom");

            entity.Property(e => e.CDel)
                .HasDefaultValue(false)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TplanCourseSectionEnrollment>(entity =>
        {
            entity.HasKey(e => e.PlanCourseEnrollmentId).HasName("TPlanCourseSectionEnrollment_pkey");

            entity.ToTable("TPlanCourseSectionEnrollment");

            entity.Property(e => e.CDel)
                .HasDefaultValue(false)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.EnrollStatus).HasMaxLength(20);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TplanCourseSectionPeriod>(entity =>
        {
            entity.HasKey(e => e.PlanCourseSectionPeriodId).HasName("TPlanCourseSectionPeriod_pkey");

            entity.ToTable("TPlanCourseSectionPeriod");

            entity.Property(e => e.CDel)
                .HasDefaultValue(false)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.NCredit).HasColumnName("nCredit");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TplanCourseSetting>(entity =>
        {
            entity.HasKey(e => e.PlanCourseSettingId).HasName("TPlanCourseSetting_pkey");

            entity.ToTable("TPlanCourseSetting");

            entity.Property(e => e.CDel)
                .HasDefaultValue(false)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsRegistrationOpen).HasDefaultValue(true);
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.RegistrationEnd).HasColumnType("timestamp without time zone");
            entity.Property(e => e.RegistrationStart).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TplanCourseStudent>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.PlanCourseStudentId }).HasName("public_TPlanCourseStudent_PK_TPlanCourseStudent");

            entity.ToTable("TPlanCourseStudent");

            entity.HasIndex(e => new { e.SId, e.PlanCourseId, e.SchoolId, e.NTerm, e.IsActive, e.NTermSubLevel2, e.NTsubLevel }, "public_TPlanCourseStudent_IDX_M_16454_16453");

            entity.HasIndex(e => new { e.IsActive, e.PlanCourseId, e.SId, e.NTerm, e.NTermSubLevel2, e.NTsubLevel }, "public_TPlanCourseStudent_IDX_M_4350_4349");

            entity.HasIndex(e => new { e.PlanCourseId, e.IsActive }, "public_TPlanCourseStudent_INDEX_VV_3767_3766");

            entity.HasIndex(e => e.PlanCourseId, "public_TPlanCourseStudent_INDEX_VV_5224395_5224394");

            entity.HasIndex(e => e.PlanCourseId, "public_TPlanCourseStudent_INDEX_VV_53306_53305");

            entity.HasIndex(e => new { e.SchoolId, e.IsActive }, "public_TPlanCourseStudent_INDEX_VV_89991_89990");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.IsActive, e.NTermSubLevel2, e.NTsubLevel }, "public_TPlanCourseStudent_INDEX_VV_90061_90060");

            entity.HasIndex(e => new { e.PlanCourseId, e.SchoolId, e.NTerm, e.IsActive, e.NTermSubLevel2, e.NTsubLevel }, "public_TPlanCourseStudent_INDEX_VV_98742_98741");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.PlanCourseStudentId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTerm)
                .HasMaxLength(40)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TplanCourseTeacher>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.PlanCourseTeacherId }).HasName("public_TPlanCourseTeacher_PK_TPlanCourseTeacher");

            entity.ToTable("TPlanCourseTeacher");

            entity.HasIndex(e => new { e.IsActive, e.SchoolId }, "public_TPlanCourseTeacher_INDEX_VV_116516_116515");

            entity.HasIndex(e => new { e.IsActive, e.SchoolId, e.PlanCourseId }, "public_TPlanCourseTeacher_INDEX_VV_23647_23646");

            entity.HasIndex(e => new { e.PlanCourseId, e.IsActive, e.SchoolId, e.CDel }, "public_TPlanCourseTeacher_INDEX_VV_5130749_5130748");

            entity.HasIndex(e => new { e.SEmp, e.IsActive, e.SchoolId, e.PlanCourseId }, "public_TPlanCourseTeacher_INDEX_VV_72615_72614");

            entity.HasIndex(e => new { e.IsActive, e.SchoolId, e.CDel }, "public_TPlanCourseTeacher_INDEX_VV_92906_92905");

            entity.HasIndex(e => e.PlanCourseId, "public_TPlanCourseTeacher_IX_TPlanCourseTeacher_IsActive_School");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.PlanCourseTeacherId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TplanCourseTerm>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.PlanCourseTermId }).HasName("public_TPlanCourseTerm_PK_TPlanCourseTerm");

            entity.ToTable("TPlanCourseTerm");

            entity.HasIndex(e => new { e.PlanCourseId, e.NTerm, e.SchoolId }, "idx_plan_term");

            entity.HasIndex(e => e.PlanCourseId, "public_TPlanCourseTerm_IDX_M_48351_48350");

            entity.HasIndex(e => new { e.NTerm, e.IsActive, e.SchoolId, e.PlanCourseId }, "public_TPlanCourseTerm_INDEX_VV_120488_120487");

            entity.HasIndex(e => new { e.NTerm, e.SchoolId }, "public_TPlanCourseTerm_INDEX_VV_124447_124446");

            entity.HasIndex(e => e.PlanCourseId, "public_TPlanCourseTerm_INDEX_VV_60273_60272");

            entity.HasIndex(e => new { e.IsActive, e.SchoolId, e.NTerm }, "public_TPlanCourseTerm_INDEX_VV_98092_98091");

            entity.HasIndex(e => new { e.PlanCourseId, e.NTerm }, "public_TPlanCourseTerm_IX_TPlanCourseTerm");

            entity.HasIndex(e => new { e.PlanCourseId, e.NTerm }, "public_TPlanCourseTerm_IX_TPlanCourseTerm_Filtered");

            entity.HasIndex(e => new { e.SchoolId, e.IsActive, e.PlanCourseId, e.NTerm }, "public_TPlanCourseTerm_IX_TPlanCourseTerm_Optim");

            entity.HasIndex(e => e.PlanCourseId, "public_TPlanCourseTerm_IX_TPlanCourseTerm_nTerm_SchoolID");

            entity.HasIndex(e => new { e.NTerm, e.IsActive, e.SchoolId }, "public_TPlanCourseTerm_TPlanCourseTerm_nTerm_iSActive_SchoolID");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.PlanCourseTermId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TplanFeature>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.FeatureId }).HasName("TPlan_Feature_pkey");

            entity.ToTable("TPlan_Feature");

            entity.HasIndex(e => e.FeatureId, "TPlan_Feature_feature_id_idx");

            entity.HasIndex(e => new { e.PlanId, e.SortOrder }, "TPlan_Feature_plan_id_sort_order_idx");

            entity.HasIndex(e => new { e.SchoolId, e.PlanId, e.SortOrder }, "TPlan_Feature_schoolID_plan_id_sort_order_idx");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.FeatureId).HasColumnName("feature_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.DisplayDescription).HasColumnName("display_description");
            entity.Property(e => e.DisplayIcon)
                .HasMaxLength(20)
                .HasColumnName("display_icon");
            entity.Property(e => e.DisplayTitle)
                .HasMaxLength(150)
                .HasColumnName("display_title");
            entity.Property(e => e.SchoolId)
                .HasMaxLength(50)
                .HasColumnName("schoolID");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<TplanOwner>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NPlanOwnerId }).HasName("public_TPlanOwner_PK_TPlanTeacher");

            entity.ToTable("TPlanOwner");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NPlanOwnerId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nPlanOwnerID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.ChangeScoreAnytime).HasColumnName("changeScoreAnytime");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEMP");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TplanTermSubLevel2>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.PlanTermSubLevel2Id }).HasName("public_TPlanTermSubLevel2_PK_TPlanTermSubLevel2");

            entity.ToTable("TPlanTermSubLevel2");

            entity.HasIndex(e => new { e.PlanId, e.SchoolId, e.NTermSubLevel2 }, "idx_term_sublevel");

            entity.HasIndex(e => new { e.NTermSubLevel2, e.IsActive, e.SchoolId }, "public_TPlanTermSubLevel2_IDX_M_3792_3791");

            entity.HasIndex(e => new { e.PlanId, e.IsActive, e.SchoolId, e.NTermSubLevel2 }, "public_TPlanTermSubLevel2_IDX_M_4988_4987");

            entity.HasIndex(e => e.PlanTermSubLevel2Id, "public_TPlanTermSubLevel2_INDEX_VV_1028_1027");

            entity.HasIndex(e => new { e.IsActive, e.SchoolId, e.PlanId }, "public_TPlanTermSubLevel2_INDEX_VV_47796_47795");

            entity.HasIndex(e => e.NTermSubLevel2, "public_TPlanTermSubLevel2_INDEX_VV_56707_56706");

            entity.HasIndex(e => e.NTermSubLevel2, "public_TPlanTermSubLevel2_INDEX_VV_PLANID_ISACTIVE_SchoolID");

            entity.HasIndex(e => new { e.PlanId, e.NTermSubLevel2 }, "public_TPlanTermSubLevel2_IX_TPlanTermSubLevel2_1");

            entity.HasIndex(e => new { e.SchoolId, e.NTermSubLevel2, e.IsActive }, "public_TPlanTermSubLevel2_IX_TPlanTermSubLevel2_Optim");

            entity.HasIndex(e => e.PlanId, "public_TPlanTermSubLevel2_IX_VV_TPlanTermSubLevel2_IsActive_Sch");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.PlanTermSubLevel2Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Tplane>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SPlaneId }).HasName("public_TPlane_PK_TPlane");

            entity.ToTable("TPlane");

            entity.HasIndex(e => new { e.SchoolId, e.SPlaneId }, "idx_plane_school");

            entity.HasIndex(e => e.SPlaneName, "public_TPlane_IDX_M_1481_1480");

            entity.HasIndex(e => new { e.SchoolId, e.NTsubLevel }, "public_TPlane_INDEX_VV_100337_100336");

            entity.HasIndex(e => e.SPlaneId, "public_TPlane_INDEX_VV_12048_12047");

            entity.HasIndex(e => e.NTsubLevel, "public_TPlane_INDEX_VV_12484_12483");

            entity.HasIndex(e => e.CourseCode, "public_TPlane_INDEX_VV_126390_126389");

            entity.HasIndex(e => new { e.CDel, e.SchoolId }, "public_TPlane_INDEX_VV_2901_2900");

            entity.HasIndex(e => new { e.CDel, e.SchoolId }, "public_TPlane_INDEX_VV_91095_91094");

            entity.HasIndex(e => new { e.CDel, e.SchoolId, e.NTsubLevel }, "public_TPlane_INDEX_VV_94473_94472");

            entity.HasIndex(e => new { e.CourseGroup, e.CourseCode, e.CourseType, e.NTerm, e.NTsubLevel, e.SortNumber }, "public_TPlane_IX_TPlane");

            entity.HasIndex(e => e.SPlaneId, "public_TPlane_IX_TPlane_Filtered");

            entity.HasIndex(e => new { e.SchoolId, e.NTsubLevel, e.SPlaneId }, "public_TPlane_IX_TPlane_Optim");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SPlaneId)
                .ValueGeneratedOnAdd()
                .HasColumnName("sPlaneID");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(250)
                .HasColumnName("courseCode");
            entity.Property(e => e.CourseCodeEn).HasMaxLength(100);
            entity.Property(e => e.CourseGroup).HasColumnName("courseGroup");
            entity.Property(e => e.CourseHour).HasColumnName("courseHour");
            entity.Property(e => e.CourseNameEn).HasMaxLength(500);
            entity.Property(e => e.CourseStatus).HasColumnName("courseStatus");
            entity.Property(e => e.CourseTotalHour).HasColumnName("courseTotalHour");
            entity.Property(e => e.CourseType).HasColumnName("courseType");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NCredit).HasColumnName("nCredit");
            entity.Property(e => e.NTerm).HasColumnName("nTerm");
            entity.Property(e => e.NTsubLevel)
                .HasMaxLength(50)
                .HasColumnName("nTSubLevel");
            entity.Property(e => e.SPlaneName)
                .HasMaxLength(250)
                .HasColumnName("sPlaneName");
            entity.Property(e => e.SortNumber).HasColumnName("sortNumber");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TpreRegister>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.PreRegisterId }).HasName("public_TPreRegister_PK_TPreRegister");

            entity.ToTable("TPreRegister");

            entity.HasIndex(e => new { e.SchoolId, e.RegisterExamRoomId }, "IDX_TPreRegister_SchoolID_RegisterExamRoomID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.SchoolId }, "IDX_TPreRegister_cDel_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.OptionLevel, e.StudentType, e.RegisterPlanSetupId, e.SchoolId }, "IDX_TPreRegister_cDel_optionLevel_StudentType_RegisterPlanSetup")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.PaymentStatus, e.RegisterYear, e.SchoolId }, "IDX_TPreRegister_cDel_paymentStatus_registerYear_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.RegisterYear, e.SchoolId }, "IDX_TPreRegister_cDel_registerYear_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.RegisterYear, e.OptionCourse, e.SchoolId }, "IDX_TPreRegister_cDel_registerYear_optionCourse_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.RegisterYear, e.OptionLevel, e.OptionTime, e.SchoolId }, "IDX_TPreRegister_cDel_registerYear_optionLevel_optionTime_Schoo")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.OptionLevel, e.SchoolId }, "IDX_TPreRegister_optionLevel_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.PreRegisterId, "IDX_TPreRegister_preRegisterId")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SIdentification, e.SchoolId }, "IDX_TPreRegister_sIdentification_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SIdentification, e.RegisterYear, e.SchoolId }, "IDX_TPreRegister_sIdentification_registerYear_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SName, e.SLastname, e.CDel }, "IDX_TPreRegister_sName_sLastname_cDel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SStudentId, e.SchoolId }, "IDX_TPreRegister_sStudentID_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.RegisterProgramSetupId, "IX_TPreRegister_RegisterProgramSetupID");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.PreRegisterId)
                .ValueGeneratedOnAdd()
                .HasColumnName("preRegisterId");
            entity.Property(e => e.AddDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("addDate");
            entity.Property(e => e.AddressLat).HasColumnName("addressLat");
            entity.Property(e => e.AddressLng).HasColumnName("addressLng");
            entity.Property(e => e.BackupPlans).HasMaxLength(1200);
            entity.Property(e => e.BornFrom)
                .HasMaxLength(100)
                .HasColumnName("bornFrom");
            entity.Property(e => e.BornFromAumpher).HasColumnName("bornFromAumpher");
            entity.Property(e => e.BornFromProvince).HasColumnName("bornFromProvince");
            entity.Property(e => e.BornFromTumbon).HasColumnName("bornFromTumbon");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CSex)
                .HasMaxLength(1)
                .HasColumnName("cSex");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.CompleteDocuments).HasMaxLength(1);
            entity.Property(e => e.CompleteDocumentsInfo).HasMaxLength(1000);
            entity.Property(e => e.CompleteDocumentsUpdateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DBirth)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dBirth");
            entity.Property(e => e.DFamilyBirthDay)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dFamilyBirthDay");
            entity.Property(e => e.DFatherBirthDay)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dFatherBirthDay");
            entity.Property(e => e.DMotherBirthDay)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dMotherBirthDay");
            entity.Property(e => e.DPicUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dPicUpdate");
            entity.Property(e => e.DisabilityCardExpireDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DisabilityCardNo).HasMaxLength(20);
            entity.Property(e => e.DisabilityCertIssueDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DisabilityCode).HasMaxLength(2);
            entity.Property(e => e.ExamCode).HasMaxLength(20);
            entity.Property(e => e.ExamResults).HasMaxLength(1);
            entity.Property(e => e.ExamResultsUpdateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ExamSeatNo).HasMaxLength(20);
            entity.Property(e => e.FamilyStatus).HasColumnName("familyStatus");
            entity.Property(e => e.FatherAnnualIncome).HasMaxLength(30);
            entity.Property(e => e.FatherEmail).HasMaxLength(100);
            entity.Property(e => e.FatherIncome)
                .HasMaxLength(50)
                .HasColumnName("fatherIncome");
            entity.Property(e => e.FriendLastName)
                .HasMaxLength(100)
                .HasColumnName("friendLastName");
            entity.Property(e => e.FriendName)
                .HasMaxLength(100)
                .HasColumnName("friendName");
            entity.Property(e => e.FriendPhone)
                .HasMaxLength(30)
                .HasColumnName("friendPhone");
            entity.Property(e => e.FriendSid).HasColumnName("friendSID");
            entity.Property(e => e.FriendSubLevel).HasColumnName("friendSubLevel");
            entity.Property(e => e.HouseRegistrationAumpher).HasColumnName("houseRegistrationAumpher");
            entity.Property(e => e.HouseRegistrationMuu)
                .HasMaxLength(100)
                .HasColumnName("houseRegistrationMuu");
            entity.Property(e => e.HouseRegistrationNumber)
                .HasMaxLength(20)
                .HasColumnName("houseRegistrationNumber");
            entity.Property(e => e.HouseRegistrationPhone)
                .HasMaxLength(20)
                .HasColumnName("houseRegistrationPhone");
            entity.Property(e => e.HouseRegistrationPost)
                .HasMaxLength(20)
                .HasColumnName("houseRegistrationPost");
            entity.Property(e => e.HouseRegistrationProvince).HasColumnName("houseRegistrationProvince");
            entity.Property(e => e.HouseRegistrationRoad)
                .HasMaxLength(100)
                .HasColumnName("houseRegistrationRoad");
            entity.Property(e => e.HouseRegistrationSoy)
                .HasMaxLength(100)
                .HasColumnName("houseRegistrationSoy");
            entity.Property(e => e.HouseRegistrationTumbon).HasColumnName("houseRegistrationTumbon");
            entity.Property(e => e.KnowFrom1).HasColumnName("knowFrom1");
            entity.Property(e => e.KnowFrom10).HasColumnName("knowFrom10");
            entity.Property(e => e.KnowFrom11).HasColumnName("knowFrom11");
            entity.Property(e => e.KnowFrom2).HasColumnName("knowFrom2");
            entity.Property(e => e.KnowFrom3).HasColumnName("knowFrom3");
            entity.Property(e => e.KnowFrom4).HasColumnName("knowFrom4");
            entity.Property(e => e.KnowFrom5).HasColumnName("knowFrom5");
            entity.Property(e => e.KnowFrom5txt)
                .HasMaxLength(200)
                .HasColumnName("knowFrom5txt");
            entity.Property(e => e.KnowFrom6).HasColumnName("knowFrom6");
            entity.Property(e => e.KnowFrom7).HasColumnName("knowFrom7");
            entity.Property(e => e.KnowFrom7txt)
                .HasMaxLength(200)
                .HasColumnName("knowFrom7txt");
            entity.Property(e => e.KnowFrom8).HasColumnName("knowFrom8");
            entity.Property(e => e.KnowFrom8txt)
                .HasMaxLength(200)
                .HasColumnName("knowFrom8txt");
            entity.Property(e => e.KnowFrom9).HasColumnName("knowFrom9");
            entity.Property(e => e.KnowFrom9txt)
                .HasMaxLength(200)
                .HasColumnName("knowFrom9txt");
            entity.Property(e => e.MotherAnnualIncome).HasMaxLength(30);
            entity.Property(e => e.MotherEmail).HasMaxLength(100);
            entity.Property(e => e.MotherIncome)
                .HasMaxLength(50)
                .HasColumnName("motherIncome");
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutReason)
                .HasMaxLength(200)
                .HasColumnName("moveOutReason");
            entity.Property(e => e.NFamilyIncome).HasColumnName("nFamilyIncome");
            entity.Property(e => e.NFamilyRequestStudyMoney).HasColumnName("nFamilyRequestStudyMoney");
            entity.Property(e => e.NFamilyTitle).HasColumnName("nFamilyTitle");
            entity.Property(e => e.NFatherIncome).HasColumnName("nFatherIncome");
            entity.Property(e => e.NHeight).HasColumnName("nHeight");
            entity.Property(e => e.NMotherIncome).HasColumnName("nMotherIncome");
            entity.Property(e => e.NPicversion).HasColumnName("nPicversion");
            entity.Property(e => e.NRelativeStudyHere).HasColumnName("nRelativeStudyHere");
            entity.Property(e => e.NSonNumber).HasColumnName("nSonNumber");
            entity.Property(e => e.NSonTotal).HasColumnName("nSonTotal");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NWeight).HasColumnName("nWeight");
            entity.Property(e => e.OldSchoolAumpher)
                .HasMaxLength(50)
                .HasColumnName("oldSchoolAumpher");
            entity.Property(e => e.OldSchoolGpa).HasColumnName("oldSchoolGPA");
            entity.Property(e => e.OldSchoolGraduated)
                .HasMaxLength(2)
                .HasColumnName("oldSchoolGraduated");
            entity.Property(e => e.OldSchoolName)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolName");
            entity.Property(e => e.OldSchoolProvince)
                .HasMaxLength(50)
                .HasColumnName("oldSchoolProvince");
            entity.Property(e => e.OldSchoolTumbon)
                .HasMaxLength(50)
                .HasColumnName("oldSchoolTumbon");
            entity.Property(e => e.OptionBranch).HasColumnName("optionBranch");
            entity.Property(e => e.OptionCourse).HasColumnName("optionCourse");
            entity.Property(e => e.OptionLevel).HasColumnName("optionLevel");
            entity.Property(e => e.OptionTime).HasColumnName("optionTime");
            entity.Property(e => e.ParentAnnualIncome).HasMaxLength(30);
            entity.Property(e => e.ParentEmail).HasMaxLength(100);
            entity.Property(e => e.PaymentStatus).HasColumnName("paymentStatus");
            entity.Property(e => e.RegisterCode)
                .HasMaxLength(20)
                .HasColumnName("registerCode");
            entity.Property(e => e.RegisterExamRoomId).HasColumnName("RegisterExamRoomID");
            entity.Property(e => e.RegisterPlanSetupId).HasColumnName("RegisterPlanSetupID");
            entity.Property(e => e.RegisterProgramSetupId).HasColumnName("RegisterProgramSetupID");
            entity.Property(e => e.RegisterStatus).HasColumnName("registerStatus");
            entity.Property(e => e.RegisterYear).HasColumnName("registerYear");
            entity.Property(e => e.ReserveFromSid).HasColumnName("ReserveFromSID");
            entity.Property(e => e.SAddress)
                .HasMaxLength(512)
                .HasColumnName("sAddress");
            entity.Property(e => e.SBlood)
                .HasMaxLength(10)
                .HasColumnName("sBlood");
            entity.Property(e => e.SCity)
                .HasMaxLength(512)
                .HasColumnName("sCity");
            entity.Property(e => e.SCountry)
                .HasMaxLength(512)
                .HasColumnName("sCountry");
            entity.Property(e => e.SEmail)
                .HasMaxLength(256)
                .HasColumnName("sEmail");
            entity.Property(e => e.SFamilyAumpher)
                .HasMaxLength(50)
                .HasColumnName("sFamilyAumpher");
            entity.Property(e => e.SFamilyGraduated).HasColumnName("sFamilyGraduated");
            entity.Property(e => e.SFamilyHomeNumber)
                .HasMaxLength(50)
                .HasColumnName("sFamilyHomeNumber");
            entity.Property(e => e.SFamilyIdCardNumber)
                .HasMaxLength(20)
                .HasColumnName("sFamilyIdCardNumber");
            entity.Property(e => e.SFamilyJob)
                .HasMaxLength(200)
                .HasColumnName("sFamilyJob");
            entity.Property(e => e.SFamilyLast)
                .HasMaxLength(50)
                .HasColumnName("sFamilyLast");
            entity.Property(e => e.SFamilyLastEn)
                .HasMaxLength(50)
                .HasColumnName("sFamilyLastEN");
            entity.Property(e => e.SFamilyMuu)
                .HasMaxLength(50)
                .HasColumnName("sFamilyMuu");
            entity.Property(e => e.SFamilyName)
                .HasMaxLength(50)
                .HasColumnName("sFamilyName");
            entity.Property(e => e.SFamilyNameEn)
                .HasMaxLength(50)
                .HasColumnName("sFamilyNameEN");
            entity.Property(e => e.SFamilyNation)
                .HasMaxLength(50)
                .HasColumnName("sFamilyNation");
            entity.Property(e => e.SFamilyPost)
                .HasMaxLength(50)
                .HasColumnName("sFamilyPost");
            entity.Property(e => e.SFamilyProvince)
                .HasMaxLength(50)
                .HasColumnName("sFamilyProvince");
            entity.Property(e => e.SFamilyRace)
                .HasMaxLength(50)
                .HasColumnName("sFamilyRace");
            entity.Property(e => e.SFamilyRelate)
                .HasMaxLength(20)
                .HasColumnName("sFamilyRelate");
            entity.Property(e => e.SFamilyReligion)
                .HasMaxLength(50)
                .HasColumnName("sFamilyReligion");
            entity.Property(e => e.SFamilyRoad)
                .HasMaxLength(50)
                .HasColumnName("sFamilyRoad");
            entity.Property(e => e.SFamilySoy)
                .HasMaxLength(50)
                .HasColumnName("sFamilySoy");
            entity.Property(e => e.SFamilyTumbon)
                .HasMaxLength(50)
                .HasColumnName("sFamilyTumbon");
            entity.Property(e => e.SFamilyWorkPlace)
                .HasMaxLength(200)
                .HasColumnName("sFamilyWorkPlace");
            entity.Property(e => e.SFatherAumpher)
                .HasMaxLength(50)
                .HasColumnName("sFatherAumpher");
            entity.Property(e => e.SFatherFirstName)
                .HasMaxLength(50)
                .HasColumnName("sFatherFirstName");
            entity.Property(e => e.SFatherGraduated).HasColumnName("sFatherGraduated");
            entity.Property(e => e.SFatherHomeNumber)
                .HasMaxLength(50)
                .HasColumnName("sFatherHomeNumber");
            entity.Property(e => e.SFatherIdCardNumber)
                .HasMaxLength(50)
                .HasColumnName("sFatherIdCardNumber");
            entity.Property(e => e.SFatherJob)
                .HasMaxLength(100)
                .HasColumnName("sFatherJob");
            entity.Property(e => e.SFatherLastEn)
                .HasMaxLength(100)
                .HasColumnName("sFatherLastEN");
            entity.Property(e => e.SFatherLastName)
                .HasMaxLength(50)
                .HasColumnName("sFatherLastName");
            entity.Property(e => e.SFatherMuu)
                .HasMaxLength(50)
                .HasColumnName("sFatherMuu");
            entity.Property(e => e.SFatherNameEn)
                .HasMaxLength(100)
                .HasColumnName("sFatherNameEN");
            entity.Property(e => e.SFatherNation)
                .HasMaxLength(50)
                .HasColumnName("sFatherNation");
            entity.Property(e => e.SFatherPhone)
                .HasMaxLength(50)
                .HasColumnName("sFatherPhone");
            entity.Property(e => e.SFatherPhone2)
                .HasMaxLength(20)
                .HasColumnName("sFatherPhone2");
            entity.Property(e => e.SFatherPhone3)
                .HasMaxLength(20)
                .HasColumnName("sFatherPhone3");
            entity.Property(e => e.SFatherPost)
                .HasMaxLength(50)
                .HasColumnName("sFatherPost");
            entity.Property(e => e.SFatherProvince)
                .HasMaxLength(50)
                .HasColumnName("sFatherProvince");
            entity.Property(e => e.SFatherRace)
                .HasMaxLength(50)
                .HasColumnName("sFatherRace");
            entity.Property(e => e.SFatherReligion)
                .HasMaxLength(50)
                .HasColumnName("sFatherReligion");
            entity.Property(e => e.SFatherRoad)
                .HasMaxLength(50)
                .HasColumnName("sFatherRoad");
            entity.Property(e => e.SFatherSoy)
                .HasMaxLength(50)
                .HasColumnName("sFatherSoy");
            entity.Property(e => e.SFatherTumbon)
                .HasMaxLength(50)
                .HasColumnName("sFatherTumbon");
            entity.Property(e => e.SFatherWorkPlace)
                .HasMaxLength(200)
                .HasColumnName("sFatherWorkPlace");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SMotherAumpher)
                .HasMaxLength(50)
                .HasColumnName("sMotherAumpher");
            entity.Property(e => e.SMotherFirstName)
                .HasMaxLength(50)
                .HasColumnName("sMotherFirstName");
            entity.Property(e => e.SMotherGraduated).HasColumnName("sMotherGraduated");
            entity.Property(e => e.SMotherHomeNumber)
                .HasMaxLength(50)
                .HasColumnName("sMotherHomeNumber");
            entity.Property(e => e.SMotherIdCardNumber)
                .HasMaxLength(50)
                .HasColumnName("sMotherIdCardNumber");
            entity.Property(e => e.SMotherJob)
                .HasMaxLength(100)
                .HasColumnName("sMotherJob");
            entity.Property(e => e.SMotherLastEn)
                .HasMaxLength(100)
                .HasColumnName("sMotherLastEN");
            entity.Property(e => e.SMotherLastName)
                .HasMaxLength(50)
                .HasColumnName("sMotherLastName");
            entity.Property(e => e.SMotherMuu)
                .HasMaxLength(50)
                .HasColumnName("sMotherMuu");
            entity.Property(e => e.SMotherNameEn)
                .HasMaxLength(100)
                .HasColumnName("sMotherNameEN");
            entity.Property(e => e.SMotherNation)
                .HasMaxLength(50)
                .HasColumnName("sMotherNation");
            entity.Property(e => e.SMotherPhone)
                .HasMaxLength(50)
                .HasColumnName("sMotherPhone");
            entity.Property(e => e.SMotherPhone2)
                .HasMaxLength(20)
                .HasColumnName("sMotherPhone2");
            entity.Property(e => e.SMotherPhone3)
                .HasMaxLength(20)
                .HasColumnName("sMotherPhone3");
            entity.Property(e => e.SMotherPost)
                .HasMaxLength(50)
                .HasColumnName("sMotherPost");
            entity.Property(e => e.SMotherProvince)
                .HasMaxLength(50)
                .HasColumnName("sMotherProvince");
            entity.Property(e => e.SMotherRace)
                .HasMaxLength(50)
                .HasColumnName("sMotherRace");
            entity.Property(e => e.SMotherReligion)
                .HasMaxLength(50)
                .HasColumnName("sMotherReligion");
            entity.Property(e => e.SMotherRoad)
                .HasMaxLength(50)
                .HasColumnName("sMotherRoad");
            entity.Property(e => e.SMotherSoy)
                .HasMaxLength(50)
                .HasColumnName("sMotherSoy");
            entity.Property(e => e.SMotherTumbon)
                .HasMaxLength(50)
                .HasColumnName("sMotherTumbon");
            entity.Property(e => e.SMotherWorkPlace)
                .HasMaxLength(200)
                .HasColumnName("sMotherWorkPlace");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(50)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(100)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SPhone)
                .HasMaxLength(20)
                .HasColumnName("sPhone");
            entity.Property(e => e.SPhoneMail)
                .HasMaxLength(50)
                .HasColumnName("sPhoneMail");
            entity.Property(e => e.SPhoneOne)
                .HasMaxLength(30)
                .HasColumnName("sPhoneOne");
            entity.Property(e => e.SPhoneThree)
                .HasMaxLength(30)
                .HasColumnName("sPhoneThree");
            entity.Property(e => e.SPhoneTwo)
                .HasMaxLength(30)
                .HasColumnName("sPhoneTwo");
            entity.Property(e => e.SPostalcode)
                .HasMaxLength(10)
                .HasColumnName("sPostalcode");
            entity.Property(e => e.SSickDanger)
                .HasMaxLength(250)
                .HasColumnName("sSickDanger");
            entity.Property(e => e.SSickDrug)
                .HasMaxLength(250)
                .HasColumnName("sSickDrug");
            entity.Property(e => e.SSickFood)
                .HasMaxLength(250)
                .HasColumnName("sSickFood");
            entity.Property(e => e.SSickNormal)
                .HasMaxLength(250)
                .HasColumnName("sSickNormal");
            entity.Property(e => e.SSickOther)
                .HasMaxLength(250)
                .HasColumnName("sSickOther");
            entity.Property(e => e.SStudentAumpher)
                .HasMaxLength(50)
                .HasColumnName("sStudentAumpher");
            entity.Property(e => e.SStudentHomeNumber)
                .HasMaxLength(50)
                .HasColumnName("sStudentHomeNumber");
            entity.Property(e => e.SStudentHomeRegisterCode)
                .HasMaxLength(20)
                .HasColumnName("sStudentHomeRegisterCode");
            entity.Property(e => e.SStudentHousePhone)
                .HasMaxLength(50)
                .HasColumnName("sStudentHousePhone");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentIdCardNumber)
                .HasMaxLength(20)
                .HasColumnName("sStudentIdCardNumber");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentLastOther)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastOther");
            entity.Property(e => e.SStudentMuu)
                .HasMaxLength(50)
                .HasColumnName("sStudentMuu");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentNameOther)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameOther");
            entity.Property(e => e.SStudentNation)
                .HasMaxLength(50)
                .HasColumnName("sStudentNation");
            entity.Property(e => e.SStudentPicture)
                .HasMaxLength(255)
                .HasColumnName("sStudentPicture");
            entity.Property(e => e.SStudentPost)
                .HasMaxLength(50)
                .HasColumnName("sStudentPost");
            entity.Property(e => e.SStudentProvince)
                .HasMaxLength(50)
                .HasColumnName("sStudentProvince");
            entity.Property(e => e.SStudentRace)
                .HasMaxLength(50)
                .HasColumnName("sStudentRace");
            entity.Property(e => e.SStudentReligion)
                .HasMaxLength(50)
                .HasColumnName("sStudentReligion");
            entity.Property(e => e.SStudentRoad)
                .HasMaxLength(50)
                .HasColumnName("sStudentRoad");
            entity.Property(e => e.SStudentSoy)
                .HasMaxLength(50)
                .HasColumnName("sStudentSoy");
            entity.Property(e => e.SStudentTumbon)
                .HasMaxLength(50)
                .HasColumnName("sStudentTumbon");
            entity.Property(e => e.SaveAsSid).HasColumnName("saveAsSID");
            entity.Property(e => e.StayWithEmail)
                .HasMaxLength(100)
                .HasColumnName("stayWithEmail");
            entity.Property(e => e.StayWithEmergencyCall)
                .HasMaxLength(20)
                .HasColumnName("stayWithEmergencyCall");
            entity.Property(e => e.StayWithLast)
                .HasMaxLength(100)
                .HasColumnName("stayWithLast");
            entity.Property(e => e.StayWithName)
                .HasMaxLength(100)
                .HasColumnName("stayWithName");
            entity.Property(e => e.StayWithTitle).HasColumnName("stayWithTitle");
            entity.Property(e => e.StudentCategory).HasMaxLength(1);
            entity.Property(e => e.StudentType).HasMaxLength(1);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TpreRegisterDocument>(entity =>
        {
            entity.HasKey(e => e.PreRegisterDocumentId).HasName("public_TPreRegisterDocument_PK_TPREREGISTERDOCUMENT");

            entity.ToTable("TPreRegisterDocument");

            entity.HasIndex(e => e.PreRegisterId, "public_TPreRegisterDocument_IX_TPreRegisterDocument_preRegister");

            entity.Property(e => e.PreRegisterDocumentId).HasColumnName("PreRegisterDocumentID");
            entity.Property(e => e.ContentType).HasMaxLength(200);
            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.FileName).HasMaxLength(100);
            entity.Property(e => e.FilePath).HasMaxLength(300);
            entity.Property(e => e.PreRegisterId).HasColumnName("preRegisterId");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Vfiid).HasColumnName("VFIID");
        });

        modelBuilder.Entity<TpreRegisterFieldPageSetting>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.Vfiid, e.PageScopeId });

            entity.ToTable("TPreRegisterFieldPageSetting");

            entity.HasIndex(e => new { e.SchoolId, e.PageScopeId }, "IX_TPreRegisterFieldPageSetting_School_Scope").HasFilter("(\"IsDelete\" = false)");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Vfiid).HasColumnName("VFIID");
            entity.Property(e => e.PageScopeId).HasColumnName("PageScopeID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsVisible).HasDefaultValue(true);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TpreRegisterRequiredCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("public_TPreRegisterRequiredCategory_PK_TPREREGISTERREQUIREDCATE");

            entity.ToTable("TPreRegisterRequiredCategory");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.CategoryNameEn).HasMaxLength(50);
            entity.Property(e => e.CreateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TpreRegisterRequiredField>(entity =>
        {
            entity.HasKey(e => new { e.Vfiid, e.SchoolId }).HasName("public_TPreRegisterRequiredField_PK_TPREREGISTERREQUIREDFIELD");

            entity.ToTable("TPreRegisterRequiredField");

            entity.HasIndex(e => new { e.CategoryId, e.SchoolId }, "public_TPreRegisterRequiredField_INDEX_VV_18278_18277");

            entity.HasIndex(e => e.SchoolId, "public_TPreRegisterRequiredField_INDEX_VV_TPreRegisterRequiredF");

            entity.HasIndex(e => new { e.Status, e.CategoryId, e.SchoolId }, "public_TPreRegisterRequiredField_IX_TPreRegisterRequiredField_C");

            entity.Property(e => e.Vfiid).HasColumnName("VFIID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TpreRegisterRequiredFieldInitiate>(entity =>
        {
            entity.HasKey(e => e.Vfiid).HasName("public_TPreRegisterRequiredFieldInitiate_PK_TPREREGISTERREQUIRE");

            entity.ToTable("TPreRegisterRequiredFieldInitiate");

            entity.Property(e => e.Vfiid).HasColumnName("VFIID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FieldName).HasMaxLength(200);
            entity.Property(e => e.FieldNameEn).HasMaxLength(200);
            entity.Property(e => e.InputFieldName).HasMaxLength(50);
            entity.Property(e => e.No).HasMaxLength(10);
        });

        modelBuilder.Entity<TpreRegisterSendMail>(entity =>
        {
            entity.HasKey(e => e.SendMailId).HasName("public_TPreRegisterSendMail_PK_TPreRegisterSendMail");

            entity.ToTable("TPreRegisterSendMail");

            entity.Property(e => e.SendMailId).HasColumnName("SendMailID");
            entity.Property(e => e.PreRegisterId).HasColumnName("preRegisterId");
            entity.Property(e => e.SendDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SendFrom).HasMaxLength(250);
            entity.Property(e => e.SendTo).HasMaxLength(250);
            entity.Property(e => e.Title).HasMaxLength(150);
        });

        modelBuilder.Entity<TpreRegisterSendMailAttachment>(entity =>
        {
            entity.HasKey(e => e.PreRegisterSendMailAttachmentId);

            entity.ToTable("TPreRegisterSendMailAttachment");

            entity.HasIndex(e => e.SendMailId, "IX_TPreRegisterSendMailAttachment_SendMailID");

            entity.Property(e => e.PreRegisterSendMailAttachmentId)
                .HasDefaultValueSql("nextval('\"TPreRegisterSendMailAttachmen_PreRegisterSendMailAttachment_seq\"'::regclass)")
                .HasColumnName("PreRegisterSendMailAttachmentID");
            entity.Property(e => e.ContentType).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.SendMailId).HasColumnName("SendMailID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.SendMail).WithMany(p => p.TpreRegisterSendMailAttachments)
                .HasForeignKey(d => d.SendMailId)
                .HasConstraintName("FK_TPreRegisterSendMailAttachment_TPreRegisterSendMail");
        });

        modelBuilder.Entity<TpreRegisterSurveyAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("TPreRegisterSurveyAnswer_pkey");

            entity.ToTable("TPreRegisterSurveyAnswer");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.OtherText).HasMaxLength(500);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TopicId).HasColumnName("TopicID");

            entity.HasOne(d => d.Option).WithMany(p => p.TpreRegisterSurveyAnswers)
                .HasForeignKey(d => d.OptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TPreRegisterSurveyAnswer_OptionID_fkey");
        });

        modelBuilder.Entity<TpreRegisterSurveyOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("TPreRegisterSurveyOption_pkey");

            entity.ToTable("TPreRegisterSurveyOption");

            entity.HasIndex(e => new { e.TopicId, e.IsDel, e.Order }, "idx_prereg_survey_option_topic");

            entity.HasIndex(e => new { e.TopicId, e.OptionCode }, "uq_prereg_survey_option").IsUnique();

            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OptionCode).HasMaxLength(50);
            entity.Property(e => e.OptionName).HasMaxLength(200);
            entity.Property(e => e.OptionNameEn).HasMaxLength(200);
            entity.Property(e => e.TopicId).HasColumnName("TopicID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Topic).WithMany(p => p.TpreRegisterSurveyOptions)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TPreRegisterSurveyOption_TopicID_fkey");
        });

        modelBuilder.Entity<TpreRegisterSurveyTopic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("TPreRegisterSurveyTopic_pkey");

            entity.ToTable("TPreRegisterSurveyTopic");

            entity.HasIndex(e => new { e.SchoolId, e.TopicCode }, "uq_prereg_survey_topic").IsUnique();

            entity.Property(e => e.TopicId).HasColumnName("TopicID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TopicCode).HasMaxLength(50);
            entity.Property(e => e.TopicName).HasMaxLength(200);
            entity.Property(e => e.TopicNameEn).HasMaxLength(200);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TpromoteBatchDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TPromoteBatchDetail_pkey");

            entity.ToTable("TPromoteBatchDetail");

            entity.HasIndex(e => e.BatchId, "IX_TPromoteBatchDetail_Batch");

            entity.HasIndex(e => new { e.SchoolId, e.CreatedDate }, "IX_TPromoteBatchDetail_School_Created").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BatchId).HasColumnName("BatchID");
            entity.Property(e => e.CreatedByName).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CurrentTermId).HasMaxLength(20);
            entity.Property(e => e.RolledBackDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.RolledBackStatus).HasMaxLength(20);
            entity.Property(e => e.SIds).HasColumnName("sIDs");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TargetTermId).HasMaxLength(20);
        });

        modelBuilder.Entity<TrackingDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_TrackingData_PK_TrackingData");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChOp).HasMaxLength(200);
            entity.Property(e => e.Pid).HasColumnName("PID");
            entity.Property(e => e.SPid)
                .HasMaxLength(200)
                .HasColumnName("sPID");
        });

        modelBuilder.Entity<TregisterExamRoom>(entity =>
        {
            entity.HasKey(e => new { e.RegisterExamRoomId, e.SchoolId }).HasName("public_TRegisterExamRoom_PK_TREGISTEREXAMROOM");

            entity.ToTable("TRegisterExamRoom");

            entity.Property(e => e.RegisterExamRoomId)
                .ValueGeneratedOnAdd()
                .HasColumnName("RegisterExamRoomID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ExamRoomName).HasMaxLength(150);
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.RegisterPlanSetupId).HasColumnName("RegisterPlanSetupID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TregisterExplanation>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.RegisterExplanationId }).HasName("public_TRegisterExplanation_PK_TREGISTEREXPLANATION");

            entity.ToTable("TRegisterExplanation");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.RegisterExplanationId)
                .ValueGeneratedOnAdd()
                .HasColumnName("RegisterExplanationID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TregisterPlanSetup>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.RegPlanId, e.RegisterPlanSetupId, e.NTsubLevel }).HasName("public_TRegisterPlanSetup_PK_TRegisterPlanSetup_1");

            entity.ToTable("TRegisterPlanSetup");

            entity.HasIndex(e => e.RegisterProgramSetupId, "IX_TRegisterPlanSetup_RegisterProgramSetupID");

            entity.HasIndex(e => new { e.RegisterPlanSetupId, e.NTsubLevel }, "public_TRegisterPlanSetup_INDEX_VV_24435_24434");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.RegPlanId)
                .ValueGeneratedOnAdd()
                .HasColumnName("RegPlanID");
            entity.Property(e => e.RegisterPlanSetupId).HasColumnName("RegisterPlanSetupID");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.PlanCode).HasMaxLength(10);
            entity.Property(e => e.PlanName).HasMaxLength(100);
            entity.Property(e => e.RegisterProgramSetupId).HasColumnName("RegisterProgramSetupID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TregisterProgramSetup>(entity =>
        {
            entity.HasKey(e => new { e.RegProgramId, e.RegisterProgramSetupId, e.NTsubLevel, e.SchoolId });

            entity.ToTable("TRegisterProgramSetup");

            entity.HasIndex(e => new { e.SchoolId, e.NTsubLevel }, "IX_TRegisterProgramSetup_SchoolID_nTSubLevel");

            entity.Property(e => e.RegProgramId)
                .ValueGeneratedOnAdd()
                .HasColumnName("RegProgramID");
            entity.Property(e => e.RegisterProgramSetupId).HasColumnName("RegisterProgramSetupID");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ProgramCode).HasMaxLength(10);
            entity.Property(e => e.ProgramName).HasMaxLength(100);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TregisterRegularity>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.RegisterRegularityId }).HasName("public_TRegisterRegularity_PK_TREGISTERREGULARITY");

            entity.ToTable("TRegisterRegularity");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.RegisterRegularityId)
                .ValueGeneratedOnAdd()
                .HasColumnName("RegisterRegularityID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Filename).HasMaxLength(250);
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TregisterSetup>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.RegisterSetupId }).HasName("public_TRegisterSetup_PK_TREGISTERSETUP");

            entity.ToTable("TRegisterSetup");

            entity.HasIndex(e => e.RegisterProgramSetupId, "IX_TRegisterSetup_RegisterProgramSetupID");

            entity.HasIndex(e => e.RegisterSetupId, "public_TRegisterSetup_INDEX_VV_1313_1312");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.Year, e.EndDate }, "public_TRegisterSetup_INDEX_VV_5668206_5668205");

            entity.HasIndex(e => new { e.StudentType, e.NTsubLevel, e.SchoolId, e.CDel }, "public_TRegisterSetup_INDEX_VV_5941373_5941372");

            entity.HasIndex(e => new { e.Year, e.StudentType, e.NTsubLevel, e.RegisterPlanSetupId, e.PaymentGroupId }, "public_TRegisterSetup_IX_TRegisterSetup_Year_StudentType_nTSubL");

            entity.HasIndex(e => new { e.Year, e.NTsubLevel, e.RegisterPlanSetupId }, "public_TRegisterSetup_IX_TRegisterSetup_Year_nTSubLevel_Registe");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.RegisterSetupId)
                .ValueGeneratedOnAdd()
                .HasColumnName("RegisterSetupID");
            entity.Property(e => e.AttachmentsFailExam).HasMaxLength(250);
            entity.Property(e => e.AttachmentsPassExam).HasMaxLength(250);
            entity.Property(e => e.AttachmentsSubstitute).HasMaxLength(250);
            entity.Property(e => e.BackupPlans).HasMaxLength(1200);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EndDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.MeetingDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.MeetingPlace).HasMaxLength(250);
            entity.Property(e => e.MeetingTime).HasMaxLength(5);
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.PaymentGroupId).HasColumnName("PaymentGroupID");
            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.RegisterPlanSetupId).HasColumnName("RegisterPlanSetupID");
            entity.Property(e => e.RegisterProgramSetupId).HasColumnName("RegisterProgramSetupID");
            entity.Property(e => e.StartDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.StudentType).HasMaxLength(1);
            entity.Property(e => e.SubmitDocumentDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Troom>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SRoomId }).HasName("public_TRoom_PK_TRoom");

            entity.ToTable("TRoom");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SRoomId)
                .HasMaxLength(10)
                .HasColumnName("sRoomID");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SRoomName)
                .HasMaxLength(250)
                .HasColumnName("sRoomName");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TroomChange>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.RoomChangeId }).HasName("public_TRoomChange_PK_TChangeRoom");

            entity.ToTable("TRoomChange");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.DayChange }, "public_TRoomChange_INDEX_VV_111866_111865");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "public_TRoomChange_INDEX_VV_40855_40854");

            entity.HasIndex(e => new { e.SchoolId, e.DayChange }, "public_TRoomChange_INDEX_VV_44646_44645");

            entity.HasIndex(e => e.SId, "public_TRoomChange_INDEX_VV_782_781");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.RoomChangeId)
                .HasMaxLength(100)
                .HasColumnName("RoomChangeID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Tschedule>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SScheduleId }).HasName("public_TSchedule_PK_TScheduled");

            entity.ToTable("TSchedule");

            entity.HasIndex(e => e.NPlaneDay, "public_TSchedule_IDX_M_1440_1439");

            entity.HasIndex(e => new { e.SPlaneId, e.CDel, e.SchoolId }, "public_TSchedule_IDX_M_40_39");

            entity.HasIndex(e => e.SEmp, "public_TSchedule_INDEX_VV_144_143");

            entity.HasIndex(e => new { e.NPlaneDay, e.SEmp, e.CDel }, "public_TSchedule_INDEX_VV_1684_1683");

            entity.HasIndex(e => new { e.CDel, e.SchoolId }, "public_TSchedule_INDEX_VV_32126_32125");

            entity.HasIndex(e => e.NTermTable, "public_TSchedule_INDEX_VV_7883_7882");

            entity.HasIndex(e => e.SScheduleId, "public_TSchedule_INDEX_VV_8719_8718");

            entity.HasIndex(e => new { e.SchoolId, e.SEmp }, "public_TSchedule_INDEX_VV_89919_89918");

            entity.HasIndex(e => e.CDel, "public_TSchedule_INDEX_VV_93469_93468");

            entity.HasIndex(e => new { e.CDel, e.SchoolId }, "public_TSchedule_INDEX_VV_99552_99551");

            entity.HasIndex(e => e.TStart, "public_TSchedule_IX_TSchedule_TSchedule");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SScheduleId)
                .ValueGeneratedOnAdd()
                .HasColumnName("sScheduleID");
            entity.Property(e => e.CActive).HasColumnName("cActive");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.Calculate).HasColumnName("calculate");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DTimeEndIn).HasColumnName("dTimeEnd_IN");
            entity.Property(e => e.DTimeEndOut).HasColumnName("dTimeEnd_OUT");
            entity.Property(e => e.DTimeHalf).HasColumnName("dTimeHalf");
            entity.Property(e => e.DTimeStartIn).HasColumnName("dTimeStart_IN");
            entity.Property(e => e.DTimeStartOut).HasColumnName("dTimeStart_OUT");
            entity.Property(e => e.NPlaneDay).HasColumnName("nPlaneDay");
            entity.Property(e => e.NTermTable).HasColumnName("nTermTable");
            entity.Property(e => e.NTimeLate).HasColumnName("nTimeLate");
            entity.Property(e => e.SClassId)
                .HasMaxLength(20)
                .HasColumnName("sClassID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.TEnd).HasColumnName("tEnd");
            entity.Property(e => e.TStart).HasColumnName("tStart");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TscheduleTeacher>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.ScheduleTeacherId }).HasName("public_TScheduleTeacher_PK_TScheduleTeacher");

            entity.ToTable("TScheduleTeacher");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScheduleTeacherId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SScheduleId).HasColumnName("sScheduleID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TschoolIntegration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TSchoolIntegration_pkey");

            entity.ToTable("TSchoolIntegration");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CDel)
                .HasDefaultValue(false)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(false);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TschoolRecord>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NSchoolRecordId }).HasName("public_TSchoolRecord_PK_TSchoolRecord");

            entity.ToTable("TSchoolRecord");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NSchoolRecordId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nSchoolRecordId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DAdd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dAdd");
            entity.Property(e => e.DUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dUpdate");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTsudentId).HasColumnName("nTsudentId");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TschoolRecordDetail>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NSchoolRecordId, e.SPlaneId }).HasName("public_TSchoolRecord_Detail_PK_TSchoolRecord_Detail");

            entity.ToTable("TSchoolRecord_Detail");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NSchoolRecordId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nSchoolRecordId");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Grade).HasMaxLength(20);
            entity.Property(e => e.MaxScore).HasMaxLength(20);
            entity.Property(e => e.Note).HasMaxLength(250);
            entity.Property(e => e.ReGrade).HasMaxLength(20);
            entity.Property(e => e.Score).HasMaxLength(20);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsendDataPsi>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NYear, e.SId }).HasName("public_TSendDataPSIS_PK_TSendDataPSIS");

            entity.ToTable("TSendDataPSIS");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.ResponseContent).HasMaxLength(1000);
            entity.Property(e => e.ResponseContent2).HasMaxLength(1000);
            entity.Property(e => e.SendDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SendDate2).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsendGradePsi>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NYear, e.NTerm, e.SId, e.SPlaneId }).HasName("public_TSendGradePSIS_PK_TSendGradePSIS");

            entity.ToTable("TSendGradePSIS");

            entity.HasIndex(e => e.SId, "public_TSendGradePSIS_INDEX_VV_4918_4917");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.Grade).HasMaxLength(15);
            entity.Property(e => e.ResponseContent).HasMaxLength(300);
            entity.Property(e => e.SendDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Term).HasMaxLength(10);
        });

        modelBuilder.Entity<TsendSubjectPsi>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SPlaneId }).HasName("public_TSendSubjectPSIS_PK_TSendSubjectPSIS");

            entity.ToTable("TSendSubjectPSIS");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.ResponseContent).HasMaxLength(300);
            entity.Property(e => e.SendDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsettingExtraTime>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NSettingExtraTime }).HasName("public_TSettingExtraTime_PK_TSettingExtraTime");

            entity.ToTable("TSettingExtraTime");

            entity.HasIndex(e => e.SEmp, "public_TSettingExtraTime_INDEX_VV_5898274_5898273");

            entity.HasIndex(e => new { e.CDel, e.SchoolId, e.NTerm }, "public_TSettingExtraTime_INDEX_VV_71797_71796");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NSettingExtraTime)
                .ValueGeneratedOnAdd()
                .HasColumnName("nSettingExtraTime");
            entity.Property(e => e.AddDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("addDate");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.SEmp).HasColumnName("sEMP");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UseToken).HasColumnName("useToken");
        });

        modelBuilder.Entity<Tsm>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NSms }).HasName("public_TSMS_PK_TSMS");

            entity.ToTable("TSMS");

            entity.HasIndex(e => new { e.SchoolId, e.DSend }, "public_TSMS_INDEX_VV_103245_103244");

            entity.HasIndex(e => new { e.SchoolId, e.DSend }, "public_TSMS_INDEX_VV_104676_104675");

            entity.HasIndex(e => new { e.NSms, e.CDel }, "public_TSMS_INDEX_VV_138978_138977");

            entity.HasIndex(e => e.SendFileWithOwner, "public_TSMS_INDEX_VV_703_702");

            entity.HasIndex(e => new { e.Useradd, e.SchoolId }, "public_TSMS_INDEX_VV_95030_95029");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NSms)
                .ValueGeneratedOnAdd()
                .HasColumnName("nSMS");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DSend)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dSend");
            entity.Property(e => e.IsDel).HasColumnName("isDel");
            entity.Property(e => e.NActionType).HasColumnName("nActionType");
            entity.Property(e => e.ScheduledId)
                .HasMaxLength(200)
                .HasColumnName("scheduled_id");
            entity.Property(e => e.SendFileWithOwnerOcr).HasColumnName("SendFileWithOwnerOCR");
            entity.Property(e => e.SendToLine).HasColumnName("SendToLINE");
            entity.Property(e => e.Smsall)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("SMSAll");
            entity.Property(e => e.Smsdate)
                .HasMaxLength(10)
                .HasColumnName("SMSDate");
            entity.Property(e => e.Smsdesp).HasColumnName("SMSDesp");
            entity.Property(e => e.Smsduration).HasColumnName("SMSDuration");
            entity.Property(e => e.Smsemp)
                .HasMaxLength(1)
                .HasColumnName("SMSEMP");
            entity.Property(e => e.SmsgroupId).HasColumnName("SMSGroupID");
            entity.Property(e => e.SmsgroupType).HasColumnName("SMSGroupType");
            entity.Property(e => e.Smsstatus)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("SMSStatus");
            entity.Property(e => e.SmssubType).HasColumnName("SMSSubType");
            entity.Property(e => e.Smstitle)
                .HasMaxLength(250)
                .HasColumnName("SMSTitle");
            entity.Property(e => e.Smstype).HasColumnName("SMSType");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Useradd).HasColumnName("useradd");
        });

        modelBuilder.Entity<Tsmsgroup>(entity =>
        {
            entity.HasKey(e => new { e.SmsgroupId, e.SchoolId }).HasName("public_TSMSGroup_PK_TSMSGROUP");

            entity.ToTable("TSMSGroup");

            entity.HasIndex(e => e.SmsgroupName, "public_TSMSGroup_IX_TSMSGroup_CreatedBy_CreatedDate");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "public_TSMSGroup_IX_TSMSGroup_SchoolID_cDel");

            entity.HasIndex(e => new { e.Status, e.SchoolId, e.CDel }, "public_TSMSGroup_IX_VV_TSMSGroup_Status_SchoolID_cDel");

            entity.Property(e => e.SmsgroupId)
                .ValueGeneratedOnAdd()
                .HasColumnName("SMSGroupID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.GroupDefault).HasMaxLength(20);
            entity.Property(e => e.SmsgroupName)
                .HasMaxLength(200)
                .HasColumnName("SMSGroupName");
            entity.Property(e => e.SmsgroupNameEn)
                .HasMaxLength(200)
                .HasColumnName("SMSGroupNameEn");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsmsgroupUser>(entity =>
        {
            entity.HasKey(e => new { e.SmsgroupId, e.UserId, e.SchoolId }).HasName("public_TSMSGroupUser_PK_TSMSGROUPUSER");

            entity.ToTable("TSMSGroupUser");

            entity.HasIndex(e => new { e.SmsgroupId, e.SchoolId, e.UserId }, "public_TSMSGroupUser_INDEX_VV_126481_126480");

            entity.Property(e => e.SmsgroupId).HasColumnName("SMSGroupID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsmssubLevel>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTsubLevel, e.NSmssubId }).HasName("public_TSMSSubLevel_PK_TSMSSubLevel");

            entity.ToTable("TSMSSubLevel");

            entity.HasIndex(e => e.NSms, "public_TSMSSubLevel_IX_TSMSSubLevel");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NSmssubId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nSMSSub_id");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NSms).HasColumnName("nSMS");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TstudentCardInfo>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NStudentCardInfoId }).HasName("public_TStudentCardInfo_PK_TStudentCardInfo");

            entity.ToTable("TStudentCardInfo");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NStudentCardInfoId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nStudentCardInfoId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("date");
            entity.Property(e => e.ElementName)
                .HasMaxLength(100)
                .HasColumnName("elementName");
            entity.Property(e => e.ElementValue)
                .HasMaxLength(250)
                .HasColumnName("elementValue");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TstudentClassroomHistory>(entity =>
        {
            entity.HasKey(e => new { e.NHistoryId, e.SchoolId }).HasName("public_TStudentClassroomHistory_PK_TStudentClassroomHistory");

            entity.ToTable("TStudentClassroomHistory");

            entity.HasIndex(e => e.CreatedDate, "IDX_TSCH_CreatedDate");

            entity.HasIndex(e => new { e.NHistoryId, e.SchoolId }, "IDX_TSCH_HistoryID");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "IDX_TSCH_SchoolID_sID");

            entity.HasIndex(e => new { e.NTermSubLevel2, e.SchoolId, e.CDel }, "IDX_TSCH_TermSubLevel2_School");

            entity.HasIndex(e => new { e.NTerm, e.SchoolId, e.CDel }, "IDX_TSCH_Term_School_cDel");

            entity.HasIndex(e => new { e.CDel, e.NStudentStatus }, "IDX_TSCH_cDel_StudentStatus");

            entity.HasIndex(e => new { e.SId, e.IsActive }, "IDX_TSCH_sID_IsActive");

            entity.HasIndex(e => new { e.SId, e.NTerm, e.SchoolId }, "IDX_TSCH_sID_Term_School");

            entity.HasIndex(e => e.NTerm, "idx_sch_term");

            entity.HasIndex(e => new { e.SId, e.NTerm }, "idx_tstudentclassroomhistory_sid_term_status").HasFilter("((\"cDel\" = false) AND ((\"nStudentStatus\" IS NULL) OR ((\"nStudentStatus\" < 1) OR (\"nStudentStatus\" > 7))))");

            entity.Property(e => e.NHistoryId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nHistoryId");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.MoveInDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.MoveOutDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.Note).HasMaxLength(250);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TstudentDisability>(entity =>
        {
            entity.HasKey(e => new { e.SId, e.SchoolId });

            entity.ToTable("TStudentDisability");

            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DisabilityCardExpireDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DisabilityCardNo).HasMaxLength(20);
            entity.Property(e => e.DisabilityCertIssueDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TstudentHealthGrowth>(entity =>
        {
            entity.HasKey(e => new { e.NHealthId, e.NTsubLevel, e.NMonth, e.SchoolId }).HasName("public_TStudentHealthGrowth_PK_TStudentHealthGrowth");

            entity.ToTable("TStudentHealthGrowth");

            entity.HasIndex(e => e.SchoolId, "public_TStudentHealthGrowth_INDEX_VV_12810_12809");

            entity.HasIndex(e => new { e.NTsubLevel, e.SchoolId }, "public_TStudentHealthGrowth_INDEX_VV_89959_89958");

            entity.HasIndex(e => e.SchoolId, "public_TStudentHealthGrowth_IX_TStudentHealthGrowth_SchoolID");

            entity.HasIndex(e => new { e.NTsubLevel, e.SchoolId, e.Weight, e.Height }, "public_TStudentHealthGrowth_IX_TStudentHealthGrowth_nTSubLevel_");

            entity.HasIndex(e => new { e.SchoolId, e.Weight, e.Height }, "public_TStudentHealthGrowth_IX_VV_TStudentHealthGrowth_SchoolID");

            entity.Property(e => e.NHealthId).HasColumnName("nHealthID");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NMonth).HasColumnName("nMonth");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Height).HasPrecision(6, 2);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Weight).HasPrecision(6, 2);
        });

        modelBuilder.Entity<TstudentHealthInfo>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.StudentHealthId, e.NHealthId }).HasName("public_TStudentHealthInfo_PK_TStudentHealthInfo_1");

            entity.ToTable("TStudentHealthInfo");

            entity.HasIndex(e => new { e.SId, e.SDeleted, e.SchoolId }, "public_TStudentHealthInfo_INDEX_VV_92520_92519");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentHealthId)
                .ValueGeneratedOnAdd()
                .HasColumnName("StudentHealthID");
            entity.Property(e => e.NHealthId).HasColumnName("nHealthID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.SBlood)
                .HasMaxLength(200)
                .HasColumnName("sBlood");
            entity.Property(e => e.SDeleted)
                .HasMaxLength(10)
                .HasColumnName("sDeleted");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SSickDanger)
                .HasMaxLength(250)
                .HasColumnName("sSickDanger");
            entity.Property(e => e.SSickDrug)
                .HasMaxLength(250)
                .HasColumnName("sSickDrug");
            entity.Property(e => e.SSickFood)
                .HasMaxLength(250)
                .HasColumnName("sSickFood");
            entity.Property(e => e.SSickNormal)
                .HasMaxLength(250)
                .HasColumnName("sSickNormal");
            entity.Property(e => e.SSickOther)
                .HasMaxLength(250)
                .HasColumnName("sSickOther");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TstudentHistory>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.StudentHistoryId }).HasName("public_TStudentHIstory_PK_TStudentHIstory");

            entity.ToTable("TStudentHIstory");

            entity.HasIndex(e => new { e.StudentStatus, e.SchoolId, e.CDel }, "public_TStudentHIstory_INDEX_VV_111324_111323");

            entity.HasIndex(e => new { e.NTerm, e.SchoolId }, "public_TStudentHIstory_INDEX_VV_1365_1364");

            entity.HasIndex(e => new { e.SId, e.StudentStatus, e.SchoolId, e.CDel }, "public_TStudentHIstory_INDEX_VV_5103161_5103160");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.SId }, "public_TStudentHIstory_INDEX_VV_5900_5899");

            entity.HasIndex(e => new { e.StudentStatus, e.SchoolId, e.CDel }, "public_TStudentHIstory_INDEX_VV_92933_92932");

            entity.HasIndex(e => new { e.StudentStatus, e.NTerm, e.SchoolId, e.CDel }, "public_TStudentHIstory_INDEX_VV_93863_93862");

            entity.HasIndex(e => new { e.SId, e.NTerm, e.NTermSubLevel2Old, e.NTermSubLevel2New }, "public_TStudentHIstory_IX_TStudentHIstory");

            entity.HasIndex(e => e.NTermSubLevel2Old, "public_TStudentHIstory_IX_TStudentHIstory_nTerm_SchoolID");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentHistoryId)
                .HasMaxLength(250)
                .HasColumnName("StudentHistory_ID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DProfessionalStandard)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dProfessionalStandard");
            entity.Property(e => e.DayAdd).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DayEnd).HasMaxLength(10);
            entity.Property(e => e.DayStart).HasMaxLength(10);
            entity.Property(e => e.DiplomaCode).HasMaxLength(10);
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2New).HasColumnName("nTermSubLevel2_NEW");
            entity.Property(e => e.NTermSubLevel2Old).HasColumnName("nTermSubLevel2_OLD");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.StudentStatus).HasMaxLength(5);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Useraddid).HasColumnName("USERADDID");
        });

        modelBuilder.Entity<TstudentLevel>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NStdLvId }).HasName("public_TStudentLevel_PK_TStudentLevel");

            entity.ToTable("TStudentLevel");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NStdLvId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nStdLvID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TstudentType>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.SchoolId }).HasName("public_TStudentType_PK_TStudentType_1");

            entity.ToTable("TStudentType");

            entity.HasIndex(e => new { e.SchoolId, e.Id }, "public_TStudentType_IX_TStudentType_Join");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.StudentTypeEn)
                .HasMaxLength(100)
                .HasColumnName("StudentTypeEN");
            entity.Property(e => e.StudentTypeTh)
                .HasMaxLength(100)
                .HasColumnName("StudentTypeTH");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsubLevel>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTsubLevel }).HasName("public_TSubLevel_PK_TSubLevel");

            entity.ToTable("TSubLevel");

            entity.HasIndex(e => e.MasterCode, "public_TSubLevel_INDEX_VV_120101_120100");

            entity.HasIndex(e => e.NWorkingStatus, "public_TSubLevel_INDEX_VV_128987_128986");

            entity.HasIndex(e => e.CDel, "public_TSubLevel_INDEX_VV_1525_1524");

            entity.HasIndex(e => e.SubLevel, "public_TSubLevel_INDEX_VV_4903423_4903422");

            entity.HasIndex(e => e.NWorkingStatus, "public_TSubLevel_INDEX_VV_509_508");

            entity.HasIndex(e => e.NTlevel, "public_TSubLevel_INDEX_VV_5130121_5130120");

            entity.HasIndex(e => e.NWorkingStatus, "public_TSubLevel_INDEX_VV_90980_90979");

            entity.HasIndex(e => new { e.SubLevel, e.NTlevel, e.NTimeType }, "public_TSubLevel_IX_TSubLevel");

            entity.HasIndex(e => e.NTsubLevel, "public_TSubLevel_IX_TSubLevel_SubLevelID");

            entity.HasIndex(e => new { e.SubLevel, e.NTsubLevel }, "public_TSubLevel_IX_TSubLevel_nTSubLevel");

            entity.HasIndex(e => new { e.NTsubLevel, e.SchoolId }, "public_TSubLevel_IX_TSubLevel_nTSubLevel_SchoolID");

            entity.HasIndex(e => e.NTsubLevel, "public_TSubLevel_idx_TSubLevel_nTSubLevel");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTsubLevel)
                .ValueGeneratedOnAdd()
                .HasColumnName("nTSubLevel");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .HasColumnName("fullName");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(50)
                .HasColumnName("fullNameEN");
            entity.Property(e => e.IsGraduate).HasColumnName("isGraduate");
            entity.Property(e => e.MasterCode).HasMaxLength(20);
            entity.Property(e => e.NDeleted).HasColumnName("nDeleted");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTlevel).HasColumnName("nTLevel");
            entity.Property(e => e.NWorkingStatus).HasColumnName("nWorkingStatus");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
            entity.Property(e => e.SubLevelEn)
                .HasMaxLength(10)
                .HasColumnName("SubLevelEN");
            entity.Property(e => e.SubLevelNameEn)
                .HasMaxLength(50)
                .HasColumnName("SubLevelNameEN");
            entity.Property(e => e.SubLevelNameTh)
                .HasMaxLength(50)
                .HasColumnName("SubLevelNameTH");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsubcriptionInvoice>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TSubcriptionInvoice");

            entity.HasIndex(e => e.CreatedAt, "IX_TSubcriptionInvoice_created_at");

            entity.HasIndex(e => new { e.SId, e.Status }, "IX_TSubcriptionInvoice_sID_status");

            entity.HasIndex(e => e.SubcriptionId, "IX_TSubcriptionInvoice_subcription_id");

            entity.HasIndex(e => new { e.SId, e.SubscriptionPlanId }, "UX_TSubcriptionInvoice_pending_student_plan")
                .IsUnique()
                .HasFilter("((\"sID\" IS NOT NULL) AND (subscription_plan_id IS NOT NULL) AND (subcription_id IS NULL) AND ((status)::text = ANY ((ARRAY['Pending'::character varying, 'QR_CREATED'::character varying])::text[])))");

            entity.HasIndex(e => e.TransactionId, "UX_TSubcriptionInvoice_transaction_id")
                .IsUnique()
                .HasFilter("(transaction_id IS NOT NULL)");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.ParentCode)
                .HasMaxLength(50)
                .HasColumnName("parent_code");
            entity.Property(e => e.QrExpireAt)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("qr_expire_at");
            entity.Property(e => e.QrPayload).HasColumnName("qr_payload");
            entity.Property(e => e.SId)
                .HasMaxLength(50)
                .HasColumnName("sID");
            entity.Property(e => e.SchoolId)
                .HasMaxLength(50)
                .HasColumnName("schoolID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.SubcriptionId).HasColumnName("subcription_id");
            entity.Property(e => e.SubscriptionInvoiceId)
                .ValueGeneratedOnAdd()
                .HasColumnName("subscriptionInvoice_id");
            entity.Property(e => e.SubscriptionPlanId).HasColumnName("subscription_plan_id");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<TsubcriptionPlan>(entity =>
        {
            entity.HasKey(e => e.SubscriptionPlanId).HasName("TSubcriptionPlan_pkey");

            entity.ToTable("TSubcriptionPlan");

            entity.HasIndex(e => e.PlanCode, "TSubcriptionPlan_plan_code_key").IsUnique();

            entity.HasIndex(e => new { e.SchoolId, e.IsActive }, "TSubcriptionPlan_schoolID_is_active_idx");

            entity.Property(e => e.SubscriptionPlanId).HasColumnName("subscription_plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.CycleCount)
                .HasDefaultValue(1)
                .HasColumnName("cycle_count");
            entity.Property(e => e.CycleUnit)
                .HasMaxLength(10)
                .HasColumnName("cycle_unit");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PlanCode)
                .HasMaxLength(50)
                .HasColumnName("plan_code");
            entity.Property(e => e.PlanName)
                .HasMaxLength(150)
                .HasColumnName("plan_name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.SchoolId)
                .HasMaxLength(50)
                .HasColumnName("schoolID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<Tsubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("TSubscription_pkey");

            entity.ToTable("TSubscription");

            entity.HasIndex(e => e.ParentCode, "TSubscription_parent_code_idx");

            entity.HasIndex(e => e.ExpireAt, "TSubscription_renewal_prompt_expire_at_idx").HasFilter("(renewal_prompt_pending = true)");

            entity.HasIndex(e => e.SId, "TSubscription_sID_idx");

            entity.HasIndex(e => new { e.SchoolId, e.ParentCode }, "TSubscription_schoolID_parent_code_idx");

            entity.HasIndex(e => new { e.SchoolId, e.SId, e.Status }, "TSubscription_schoolID_sID_status_idx");

            entity.HasIndex(e => new { e.SchoolId, e.Status, e.ExpireAt }, "TSubscription_schoolID_status_expire_at_idx");

            entity.HasIndex(e => new { e.Status, e.ExpireAt }, "TSubscription_status_expire_at_idx");

            entity.HasIndex(e => e.SubscriptionPlanId, "TSubscription_subscription_plan_id_idx");

            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.ExpireAt)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("expire_at");
            entity.Property(e => e.ParentCode)
                .HasMaxLength(50)
                .HasColumnName("parent_code");
            entity.Property(e => e.PurchaseBy)
                .HasMaxLength(100)
                .HasColumnName("purchase_by");
            entity.Property(e => e.RenewalPromptPending).HasColumnName("renewal_prompt_pending");
            entity.Property(e => e.SId)
                .HasMaxLength(50)
                .HasColumnName("sID");
            entity.Property(e => e.SchoolId)
                .HasMaxLength(50)
                .HasColumnName("schoolID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.SubscriptionPlanId).HasColumnName("subscription_plan_id");
            entity.Property(e => e.Survey)
                .HasMaxLength(50)
                .HasColumnName("survey");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<TsubscriptionIdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.IdempotencyKey).HasName("TSubscriptionIdempotencyKey_pkey");

            entity.ToTable("TSubscriptionIdempotencyKey");

            entity.HasIndex(e => e.CreatedAt, "IX_TSubscriptionIdempotencyKey_created_at");

            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(100)
                .HasColumnName("idempotency_key");
            entity.Property(e => e.CallerSid)
                .HasMaxLength(50)
                .HasColumnName("caller_sid");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Endpoint)
                .HasMaxLength(100)
                .HasColumnName("endpoint");
            entity.Property(e => e.ResponseBody).HasColumnName("response_body");
            entity.Property(e => e.ResponseStatus).HasColumnName("response_status");
        });

        modelBuilder.Entity<TteacherCardInfo>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TeacherCardInfoId }).HasName("public_TTeacherCardInfo_PK_TTeacherCardInfo");

            entity.ToTable("TTeacherCardInfo");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TeacherCardInfoId).ValueGeneratedOnAdd();
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("date");
            entity.Property(e => e.ElementName)
                .HasMaxLength(100)
                .HasColumnName("elementName");
            entity.Property(e => e.ElementValue)
                .HasMaxLength(250)
                .HasColumnName("elementValue");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TteacherStudentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_TTeacherStudentType_PK_TTeacherStudentType");

            entity.ToTable("TTeacherStudentType");

            entity.HasIndex(e => new { e.SchoolId, e.SEmp }, "public_TTeacherStudentType_IX_TTeacherStudentType");

            entity.HasIndex(e => e.SEmp, "public_TTeacherStudentType_IX_TTeacherStudentType_1");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.StudentTypeId).HasColumnName("StudentTypeID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TtempScanCameraIpstatus>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.CameraIp }).HasName("public_TTempScanCameraIPStatus_PK_TTempScanCameraIPStatus");

            entity.ToTable("TTempScanCameraIPStatus");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CameraIp)
                .HasMaxLength(50)
                .HasColumnName("CameraIP");
            entity.Property(e => e.CameraStatus).HasMaxLength(10);
            entity.Property(e => e.Ip)
                .HasMaxLength(50)
                .HasColumnName("IP");
            entity.Property(e => e.LastUpdate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Mac)
                .HasMaxLength(50)
                .HasColumnName("MAC");
            entity.Property(e => e.MachineName).HasMaxLength(50);
        });

        modelBuilder.Entity<TtempScanStatusOnline>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.CameraNo, e.CameraIp }).HasName("public_TTempScanStatusOnline_PK_TTempScanStatusOnline_1");

            entity.ToTable("TTempScanStatusOnline");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CameraIp)
                .HasMaxLength(50)
                .HasColumnName("CameraIP");
            entity.Property(e => e.CameraStatus).HasMaxLength(10);
            entity.Property(e => e.Ip)
                .HasMaxLength(50)
                .HasColumnName("IP");
            entity.Property(e => e.LastOnline).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Mac)
                .HasMaxLength(50)
                .HasColumnName("MAC");
            entity.Property(e => e.MachineName).HasMaxLength(50);
        });

        modelBuilder.Entity<Tterm>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTerm }).HasName("public_TTerm_PK_TTerm");

            entity.ToTable("TTerm");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm }, "IDX_TTerm_SchoolID_nTerm")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.CDel, "IDX_TTerm_cDel")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.SchoolId, e.DStart }, "IDX_TTerm_cDel_SchoolID_dStart")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.DStart, e.DEnd }, "IDX_TTerm_cDel_dStart_dEnd")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.CDel, e.STerm }, "IDX_TTerm_cDel_sTerm")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.DStart, e.DEnd }, "IDX_TTerm_dStart_dEnd")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.NTerm, "IDX_TTerm_nTerm")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.NTerm, e.SchoolId }, "IDX_TTerm_nTerm_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.NYear, e.SchoolId }, "IDX_TTerm_nYear_SchoolID")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.NYear, e.STerm, e.DStart, e.DEnd }, "IDX_TTerm_nYear_sTerm_dStart_dEnd")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.STerm, "IDX_TTerm_sTerm")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.SchoolId, e.NYear, e.NTerm }, "idx_term_school")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.DStart, e.DEnd, e.SchoolId }, "idx_tterm_active_dates").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.HasIndex(e => new { e.SchoolId, e.DStart }, "idx_tterm_school_dstart_asc").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.HasIndex(e => new { e.SchoolId, e.DStart }, "idx_tterm_school_dstart_desc")
                .IsDescending(false, true)
                .HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DEnd)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dEnd");
            entity.Property(e => e.DStart)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dStart");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.NumberTerm).HasColumnName("numberTerm");
            entity.Property(e => e.STerm)
                .HasMaxLength(100)
                .HasColumnName("sTerm");
            entity.Property(e => e.TermStatus).HasMaxLength(1);
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TtermSubLevel2>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTermSubLevel2 }).HasName("public_TTermSubLevel2_PK_TTermSubLevel2");

            entity.ToTable("TTermSubLevel2");

            entity.HasIndex(e => new { e.SchoolId, e.NTermSubLevel2 }, "idx_tsl2_school");

            entity.HasIndex(e => e.NTerm, "idx_ttermsublevel2_nterm")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.NTermSubLevel2, "idx_ttermsublevel2_ntermsublevel2")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.NTermSubLevel2, e.SchoolId }, "idx_ttermsublevel2_ntermsublevel2_schoolid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.NTsubLevel, e.SchoolId }, "idx_ttermsublevel2_ntsublevel_schoolid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.NWorkingStatus, "idx_ttermsublevel2_nworkingstatus")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => new { e.NWorkingStatus, e.SchoolId }, "idx_ttermsublevel2_nworkingstatus_schoolid")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTermSubLevel2)
                .ValueGeneratedOnAdd()
                .HasColumnName("nTermSubLevel2");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NBranchSpecId).HasColumnName("nBranchSpecId");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2Status)
                .HasMaxLength(1)
                .HasColumnName("nTermSubLevel2Status");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.NWorkingStatus).HasColumnName("nWorkingStatus");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TtermTimeTable>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTermTable }).HasName("public_TTermTimeTable_PK_TTermTimeTable");

            entity.ToTable("TTermTimeTable");

            entity.HasIndex(e => new { e.NTermSubLevel2, e.NTermTable }, "public_TTermTimeTable_IDX_M_798_797");

            entity.HasIndex(e => e.NTermTable, "public_TTermTimeTable_INDEX_VV_12246_12245");

            entity.HasIndex(e => new { e.NTerm, e.NTeacher, e.NTermSubLevel2 }, "public_TTermTimeTable_IX_TTermTimeTable");

            entity.HasIndex(e => new { e.NTermSubLevel2, e.SchoolId }, "public_TTermTimeTable_IX_TTermTimeTable_nTermSubLevel2_SchoolID");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTermTable)
                .ValueGeneratedOnAdd()
                .HasColumnName("nTermTable");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTeacher).HasColumnName("nTeacher");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Ttime>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.TimeId }).HasName("public_TTime_PK_TTime_1");

            entity.ToTable("TTime");

            entity.HasIndex(e => e.NTimeType, "public_TTime_INDEX_VV_18219_18218");

            entity.HasIndex(e => e.CDel, "public_TTime_INDEX_VV_5204601_5204600");

            entity.HasIndex(e => e.NDay, "public_TTime_INDEX_VV_96669_96668");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TimeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("TimeID");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DTimeEndIn)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeEnd_IN");
            entity.Property(e => e.DTimeEndOut)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeEnd_OUT");
            entity.Property(e => e.DTimeHalf)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeHalf");
            entity.Property(e => e.DTimeStartIn)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeStart_IN");
            entity.Property(e => e.DTimeStartOut)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dTimeStart_OUT");
            entity.Property(e => e.NDay).HasColumnName("nDay");
            entity.Property(e => e.NTimeLate).HasColumnName("nTimeLate");
            entity.Property(e => e.NTimeType).HasColumnName("nTimeType");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TtimetableProcessResult>(entity =>
        {
            entity.HasKey(e => e.TimetableProcessResultId).HasName("TTimetableProcessResult_pkey");

            entity.ToTable("TTimetableProcessResult");

            entity.HasIndex(e => e.SchoolId, "IX_TTimetableProcessResult_SchoolId");

            entity.HasIndex(e => e.CorrelationId, "UX_TTimetableProcessResult_CorrelationId").IsUnique();

            entity.Property(e => e.TimetableProcessResultId).UseIdentityAlwaysColumn();
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Nterm)
                .HasMaxLength(20)
                .HasColumnName("NTerm");
            entity.Property(e => e.NtsubLevel).HasColumnName("NTSubLevel");
            entity.Property(e => e.Nyear).HasColumnName("NYear");
            entity.Property(e => e.Payload).HasColumnType("jsonb");
            entity.Property(e => e.Status).HasMaxLength(1);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Ttimetype>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTimeType }).HasName("public_TTimetype_PK_TTimetype");

            entity.ToTable("TTimetype");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTimeType)
                .ValueGeneratedOnAdd()
                .HasColumnName("nTimeType");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.CUserType)
                .HasMaxLength(1)
                .HasColumnName("cUserType");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.STimeType)
                .HasMaxLength(100)
                .HasColumnName("sTimeType");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TtitleList>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTitleid }).HasName("public_TTitleList_PK_TTitleList");

            entity.ToTable("TTitleList");

            entity.HasIndex(e => e.NSchoolId, "public_TTitleList_INDEX_VV_5181199_5181198");

            entity.HasIndex(e => e.TitleDescription, "public_TTitleList_INDEX_VV_5254317_5254316");

            entity.HasIndex(e => new { e.NTitleid, e.SchoolId }, "public_TTitleList_IX_TTitleList_Lookup");

            entity.HasIndex(e => e.NTitleid, "public_TTitleList_IX_TTitleList_TitleID");

            entity.HasIndex(e => new { e.NTitleid, e.SchoolId }, "public_TTitleList_IX_TTitleList_nTitleid_SchoolID");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTitleid)
                .ValueGeneratedOnAdd()
                .HasColumnName("nTitleid");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Deleted)
                .HasMaxLength(50)
                .HasColumnName("deleted");
            entity.Property(e => e.MasterCode).HasMaxLength(20);
            entity.Property(e => e.NSchoolId).HasColumnName("nSchoolId");
            entity.Property(e => e.NTitleIdStr)
                .HasMaxLength(200)
                .HasColumnName("nTitleIdStr");
            entity.Property(e => e.TitleDescription)
                .HasMaxLength(100)
                .HasColumnName("titleDescription");
            entity.Property(e => e.TitleDescriptionEn)
                .HasMaxLength(100)
                .HasColumnName("titleDescriptionEn");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.WorkStatus)
                .HasMaxLength(50)
                .HasColumnName("workStatus");
        });

        modelBuilder.Entity<TtranscriptNumber>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.SchoolId, e.NTsubLevel, e.SId }).HasName("public_TTranscriptNumber_PK_TTranscriptNumber");

            entity.ToTable("TTranscriptNumber");

            entity.HasIndex(e => new { e.SchoolId, e.NTsubLevel, e.SId, e.CDel }, "public_TTranscriptNumber_INDEX_VV_29189_29188");

            entity.HasIndex(e => e.SchoolId, "public_TTranscriptNumber_INDEX_VV_3811_3810");

            entity.HasIndex(e => new { e.SchoolId, e.TranscriptSetNumber, e.TranscriptNumber }, "public_TTranscriptNumber_INDEX_VV_51494_51493");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "public_TTranscriptNumber_INDEX_VV_892_891");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Ttype>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NTypeId }).HasName("public_TType_PK_TType");

            entity.ToTable("TType");

            entity.HasIndex(e => e.NTypeId, "public_TType_INDEX_VV_2_1");

            entity.HasIndex(e => e.ShopId, "public_TType_INDEX_VV_4822461_4822460");

            entity.HasIndex(e => e.NTypeId, "public_TType_public_TType_INDEX_VV_2_1");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NTypeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("nTypeID");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SType)
                .HasMaxLength(100)
                .HasColumnName("sType");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<Tuser>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SId }).HasName("public_TUser_PK_TUser");

            entity.ToTable("TUser");

            entity.HasIndex(e => e.SId, "idx_tuser_active").HasFilter("((\"cDel\" IS NULL) OR (\"cDel\" <> '1'::bpchar))");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "idx_tuser_active_sid").HasFilter("((\"cDel\" IS NULL) OR (\"cDel\" = 'G'::bpchar))");

            entity.HasIndex(e => new { e.SIdentification, e.SchoolId }, "idx_tuser_identification");

            entity.HasIndex(e => e.SIdentification, "idx_tuser_identification_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => new { e.SchoolId, e.SName, e.SLastname }, "idx_tuser_name");

            entity.HasIndex(e => e.NTermSubLevel2, "idx_tuser_ntermsublevel2");

            entity.HasIndex(e => e.ParentCode, "idx_tuser_parent_code");

            entity.HasIndex(e => e.SchoolId, "idx_tuser_school_active").HasFilter("((\"cDel\" IS NULL) OR (\"cDel\" <> '1'::bpchar))");

            entity.HasIndex(e => e.SchoolId, "idx_tuser_school_active_d").HasFilter("(\"cDel\" IS DISTINCT FROM '1'::bpchar)");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "idx_tuser_schoolid_cdel");

            entity.HasIndex(e => e.SId, "idx_tuser_sid_active").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) = '0'::bpchar)");

            entity.HasIndex(e => e.SId, "idx_tuser_sid_cdel").HasFilter("((\"cDel\" IS NULL) OR (\"cDel\" <> '1'::bpchar))");

            entity.HasIndex(e => e.SId, "idx_tuser_sid_only");

            entity.HasIndex(e => new { e.SchoolId, e.StudentTypeId, e.NStudentStatus }, "idx_tuser_student");

            entity.HasIndex(e => new { e.SchoolId, e.SStudentId }, "idx_tuser_studentid");

            entity.HasIndex(e => e.SStudentId, "idx_tuser_studentid_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "idx_user_active_fast").HasFilter("((\"cDel\" IS NULL) OR (\"cDel\" <> '1'::bpchar))");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "idx_user_active_lookup").HasFilter("((\"cDel\" IS NULL) OR (\"cDel\" <> '1'::bpchar))");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "idx_user_covering");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "idx_user_school_active_sid").HasFilter("(COALESCE(\"cDel\", '0'::bpchar) <> '1'::bpchar)");

            entity.HasIndex(e => new { e.SchoolId, e.SId }, "uidx_tuser_schoolid_sid").IsUnique();

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.AccountNumber).HasMaxLength(30);
            entity.Property(e => e.AddressLat).HasColumnName("addressLat");
            entity.Property(e => e.AddressLng).HasColumnName("addressLng");
            entity.Property(e => e.BaseSalary).HasColumnName("baseSalary");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CSex)
                .HasMaxLength(1)
                .HasColumnName("cSex");
            entity.Property(e => e.CSms)
                .HasMaxLength(1)
                .HasColumnName("cSMS");
            entity.Property(e => e.CTelSms)
                .HasMaxLength(50)
                .HasColumnName("cTelSMS");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.CountryNameEn)
                .HasMaxLength(200)
                .HasColumnName("CountryNameEN");
            entity.Property(e => e.CountryNameTh)
                .HasMaxLength(200)
                .HasColumnName("CountryNameTH");
            entity.Property(e => e.CountryType).HasMaxLength(1);
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Credit).HasPrecision(6, 2);
            entity.Property(e => e.Credit2).HasPrecision(6, 2);
            entity.Property(e => e.DBirth)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dBirth");
            entity.Property(e => e.DPicUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dPicUpdate");
            entity.Property(e => e.DUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dUpdate");
            entity.Property(e => e.DailyUsageLimit).HasPrecision(18, 2);
            entity.Property(e => e.DiplomaCode).HasMaxLength(10);
            entity.Property(e => e.DisabilityCode).HasMaxLength(2);
            entity.Property(e => e.DisadvantageCode).HasMaxLength(2);
            entity.Property(e => e.DormitoryName).HasMaxLength(150);
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutReason)
                .HasMaxLength(200)
                .HasColumnName("moveOutReason");
            entity.Property(e => e.MoveOutReason2)
                .HasMaxLength(200)
                .HasColumnName("moveOutReason2");
            entity.Property(e => e.NMax)
                .HasPrecision(18, 2)
                .HasColumnName("nMax");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 2)
                .HasColumnName("nMoney");
            entity.Property(e => e.NPicversion).HasColumnName("nPicversion");
            entity.Property(e => e.NSonNumber).HasColumnName("nSonNumber");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.Note).HasMaxLength(250);
            entity.Property(e => e.Note2).HasMaxLength(1000);
            entity.Property(e => e.OldSchoolAumpher)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolAumpher");
            entity.Property(e => e.OldSchoolDateGraduated).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.OldSchoolDateGraduated2).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.OldSchoolGpa)
                .HasPrecision(18, 2)
                .HasColumnName("oldSchoolGPA");
            entity.Property(e => e.OldSchoolGpa2)
                .HasMaxLength(50)
                .HasColumnName("oldSchoolGPA2");
            entity.Property(e => e.OldSchoolGpa3)
                .HasPrecision(18, 2)
                .HasColumnName("oldSchoolGPA3");
            entity.Property(e => e.OldSchoolGpa4)
                .HasMaxLength(50)
                .HasColumnName("oldSchoolGPA4");
            entity.Property(e => e.OldSchoolGraduated)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolGraduated");
            entity.Property(e => e.OldSchoolGraduated2)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolGraduated2");
            entity.Property(e => e.OldSchoolName)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolName");
            entity.Property(e => e.OldSchoolName2)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolName2");
            entity.Property(e => e.OldSchoolProvince)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolProvince");
            entity.Property(e => e.OldSchoolTumbon)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolTumbon");
            entity.Property(e => e.ParentCode)
                .HasMaxLength(13)
                .HasColumnName("parent_code");
            entity.Property(e => e.PassportCountry).HasMaxLength(100);
            entity.Property(e => e.PassportExpirationDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.PassportNumber).HasMaxLength(50);
            entity.Property(e => e.SAddress)
                .HasMaxLength(512)
                .HasColumnName("sAddress");
            entity.Property(e => e.SCity)
                .HasMaxLength(512)
                .HasColumnName("sCity");
            entity.Property(e => e.SCountry)
                .HasMaxLength(512)
                .HasColumnName("sCountry");
            entity.Property(e => e.SEmail)
                .HasMaxLength(256)
                .HasColumnName("sEmail");
            entity.Property(e => e.SFinger).HasColumnName("sFinger");
            entity.Property(e => e.SFinger2).HasColumnName("sFinger2");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(256)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SPassword)
                .HasMaxLength(100)
                .HasColumnName("sPassword");
            entity.Property(e => e.SPhone)
                .HasMaxLength(100)
                .HasColumnName("sPhone");
            entity.Property(e => e.SPostalcode)
                .HasMaxLength(10)
                .HasColumnName("sPostalcode");
            entity.Property(e => e.SStudentAumpher)
                .HasMaxLength(50)
                .HasColumnName("sStudentAumpher");
            entity.Property(e => e.SStudentHomeNumber)
                .HasMaxLength(100)
                .HasColumnName("sStudentHomeNumber");
            entity.Property(e => e.SStudentHomeRegisterCode)
                .HasMaxLength(20)
                .HasColumnName("sStudentHomeRegisterCode");
            entity.Property(e => e.SStudentHousePhone)
                .HasMaxLength(50)
                .HasColumnName("sStudentHousePhone");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentIdCardNumber)
                .HasMaxLength(20)
                .HasColumnName("sStudentIdCardNumber");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentLastOther)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastOther");
            entity.Property(e => e.SStudentMuu)
                .HasMaxLength(50)
                .HasColumnName("sStudentMuu");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentNameOther)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameOther");
            entity.Property(e => e.SStudentNation)
                .HasMaxLength(50)
                .HasColumnName("sStudentNation");
            entity.Property(e => e.SStudentPicture)
                .HasMaxLength(255)
                .HasColumnName("sStudentPicture");
            entity.Property(e => e.SStudentPost)
                .HasMaxLength(50)
                .HasColumnName("sStudentPost");
            entity.Property(e => e.SStudentProvince)
                .HasMaxLength(50)
                .HasColumnName("sStudentProvince");
            entity.Property(e => e.SStudentRace)
                .HasMaxLength(50)
                .HasColumnName("sStudentRace");
            entity.Property(e => e.SStudentReligion)
                .HasMaxLength(50)
                .HasColumnName("sStudentReligion");
            entity.Property(e => e.SStudentRoad)
                .HasMaxLength(50)
                .HasColumnName("sStudentRoad");
            entity.Property(e => e.SStudentSoy)
                .HasMaxLength(50)
                .HasColumnName("sStudentSoy");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.SStudentTumbon)
                .HasMaxLength(50)
                .HasColumnName("sStudentTumbon");
            entity.Property(e => e.SSubtopic)
                .HasMaxLength(512)
                .HasColumnName("sSubtopic");
            entity.Property(e => e.SToken).HasColumnName("sToken");
            entity.Property(e => e.StudentTypeId).HasColumnName("StudentTypeID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TuserAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("public_TUserAttendance_PK_TUserAttendance");

            entity.ToTable("TUserAttendance");

            entity.HasIndex(e => new { e.SchoolId, e.ShopId, e.AttendanceDate }, "public_TUserAttendance_INDEX_VV_100569_100568");

            entity.HasIndex(e => new { e.SchoolId, e.AttendanceDate }, "public_TUserAttendance_INDEX_VV_100571_100570");

            entity.HasIndex(e => new { e.SchoolId, e.CType, e.ShopId, e.AttendanceDate }, "public_TUserAttendance_INDEX_VV_90455_90454");

            entity.HasIndex(e => new { e.SchoolId, e.SId, e.AttendanceDate }, "public_TUserAttendance_IX_TUserAttendance_SchoolID_sID_Attendan");

            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");
            entity.Property(e => e.CType).HasColumnName("cType");
            entity.Property(e => e.MacAddress).HasMaxLength(250);
            entity.Property(e => e.Nfcencrypt)
                .HasMaxLength(250)
                .HasColumnName("NFCEncrypt");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastName)
                .HasMaxLength(250)
                .HasColumnName("sLastName");
            entity.Property(e => e.SName)
                .HasMaxLength(250)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(250)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentPicture)
                .HasMaxLength(255)
                .HasColumnName("sStudentPicture");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.Tstamp)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("TStamp");
        });

        modelBuilder.Entity<TuserCard>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TUser_Card", "masterdb_schema");

            entity.Property(e => e.Created).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FreeText).HasMaxLength(20);
            entity.Property(e => e.Modified).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Nfc)
                .HasMaxLength(20)
                .HasColumnName("NFC");
            entity.Property(e => e.Nfcencrypt)
                .HasMaxLength(20)
                .HasColumnName("NFCEncrypt");
            entity.Property(e => e.NfcencryptReverse)
                .HasMaxLength(20)
                .HasColumnName("NFCEncryptReverse");
            entity.Property(e => e.Nfcreverse)
                .HasMaxLength(20)
                .HasColumnName("NFCReverse");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<TuserVerify>(entity =>
        {
            entity.HasKey(e => e.VerifyId).HasName("public_TUserVerify_PK_TUserVerify");

            entity.ToTable("TUserVerify");

            entity.HasIndex(e => new { e.SchoolId, e.UserId }, "public_TUserVerify_INDEX_VV_4983534_4983533");

            entity.HasIndex(e => new { e.UserId, e.Type, e.Status }, "public_TUserVerify_INDEX_VV_5092614_5092613");

            entity.Property(e => e.VerifyId).HasColumnName("VerifyID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmailPin).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);
            entity.Property(e => e.Ref).HasMaxLength(10);
            entity.Property(e => e.RequestDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.RequestNo).HasMaxLength(50);
            entity.Property(e => e.RequestResponse).HasMaxLength(1000);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Token).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.VerifyDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.VerifyPin).HasMaxLength(10);
            entity.Property(e => e.VerifyResponse).HasMaxLength(1000);
        });

        modelBuilder.Entity<TuserVerifyLineconnect>(entity =>
        {
            entity.HasKey(e => e.ConnectId).HasName("public_TUserVerifyLINEConnect_PK_TUserVerifyLINEConnect");

            entity.ToTable("TUserVerifyLINEConnect");

            entity.HasIndex(e => new { e.UserId, e.Status, e.LineauthorizeCode }, "public_TUserVerifyLINEConnect_INDEX_VV_5011105_5011104");

            entity.HasIndex(e => new { e.UserId, e.SchoolId, e.Status }, "public_TUserVerifyLINEConnect_INDEX_VV_5054583_5054582");

            entity.Property(e => e.ConnectId).HasColumnName("ConnectID");
            entity.Property(e => e.ConnectDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LineauthorizeCode)
                .HasMaxLength(50)
                .HasColumnName("LINEAuthorizeCode");
            entity.Property(e => e.Linetoken)
                .HasMaxLength(50)
                .HasColumnName("LINEToken");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Tyear>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.NYear }).HasName("public_TYear_PK_TYear");

            entity.ToTable("TYear");

            entity.HasIndex(e => e.NumberYear, "public_TYear_INDEX_VV_156570_156569");

            entity.HasIndex(e => new { e.NYear, e.NumberYear }, "public_TYear_INDEX_VV_31234_31233");

            entity.HasIndex(e => e.CDel, "public_TYear_INDEX_VV_53169_53168");

            entity.HasIndex(e => e.YearStatus, "public_TYear_INDEX_VV_numberyear");

            entity.HasIndex(e => new { e.NYear, e.SchoolId }, "public_TYear_IX_TYear_Joins");

            entity.HasIndex(e => e.NYear, "public_TYear_idx_TYear_nYear");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.NYear)
                .ValueGeneratedOnAdd()
                .HasColumnName("nYear");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.NumberYear).HasColumnName("numberYear");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.YearStatus).HasMaxLength(1);
        });

        modelBuilder.Entity<UpdateBalance>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("public_UpdateBalance_PK_UpdateBalance");

            entity.ToTable("UpdateBalance");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 3)
                .HasColumnName("nMoney");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<UpdateBalanceLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("public_UpdateBalanceLog_PK_UpdateBalanceLog_1");

            entity.ToTable("UpdateBalanceLog");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.NewBalance).HasPrecision(18, 3);
            entity.Property(e => e.OldBalance).HasPrecision(18, 3);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.Tstatmp)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("TStatmp");
        });

        modelBuilder.Entity<UpdateBalanceMemory>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("public_UpdateBalanceMemory_PK_UpdateBalanceMemory");

            entity.ToTable("UpdateBalanceMemory");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 3)
                .HasColumnName("nMoney");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<UpdateBalanceMemoryBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UpdateBalanceMemoryBackup");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 3)
                .HasColumnName("nMoney");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.Tstamp).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<UserAuditTable>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("User_Audit_Table_pkey");

            entity.ToTable("User_Audit_Table");

            entity.HasIndex(e => e.AuditId, "IDX_PK_AuditID").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.HasIndex(e => new { e.TableName, e.NewId, e.AuditId }, "IDX_VV_TableName_NewID")
                .IsDescending(false, true, true)
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.AuditId)
                .HasDefaultValueSql("nextval('\"User_Audit_Table_auditid_seq\"'::regclass)")
                .HasColumnName("AuditID");
            entity.Property(e => e.NewId).HasColumnName("NewID");
            entity.Property(e => e.OldId).HasColumnName("OldID");
            entity.Property(e => e.StringId).HasColumnName("StringID");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("TStamp");
        });

        modelBuilder.Entity<UserDailyBalance>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserDailyBalance", "canteen_schema");

            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.OfflineSales).HasPrecision(18, 2);
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TotalCancelSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalCancelTopUp).HasPrecision(18, 2);
            entity.Property(e => e.TotalCancelWithDraw).HasPrecision(18, 2);
            entity.Property(e => e.TotalSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalTopUp).HasPrecision(18, 2);
            entity.Property(e => e.TotalWithDraw).HasPrecision(18, 2);
        });

        modelBuilder.Entity<UserDailyBalanceBoat>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserDailyBalanceBoat", "canteen_schema");

            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.OfflineSales).HasPrecision(18, 2);
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TotalCancelSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalCancelTopUp).HasPrecision(18, 2);
            entity.Property(e => e.TotalCancelWithDraw).HasPrecision(18, 2);
            entity.Property(e => e.TotalSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalTopUp).HasPrecision(18, 2);
            entity.Property(e => e.TotalWithDraw).HasPrecision(18, 2);
        });

        modelBuilder.Entity<UserDailyBalanceTempCard>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserDailyBalanceTempCard", "canteen_schema");

            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.CardHistoryId).HasColumnName("CardHistoryID");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.OfflineSales).HasPrecision(18, 2);
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TotalCancelSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalCancelTopUp).HasPrecision(18, 2);
            entity.Property(e => e.TotalCancelWithDraw).HasPrecision(18, 2);
            entity.Property(e => e.TotalSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalTopUp).HasPrecision(18, 2);
            entity.Property(e => e.TotalWithDraw).HasPrecision(18, 2);
        });

        modelBuilder.Entity<UserDailyBalanceTempCardBoat>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserDailyBalanceTempCardBoat", "canteen_schema");

            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.CardHistoryId).HasColumnName("CardHistoryID");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.OfflineSales).HasPrecision(18, 2);
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TotalCancelSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalCancelTopUp).HasPrecision(18, 2);
            entity.Property(e => e.TotalCancelWithDraw).HasPrecision(18, 2);
            entity.Property(e => e.TotalSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalTopUp).HasPrecision(18, 2);
            entity.Property(e => e.TotalWithDraw).HasPrecision(18, 2);
        });

        modelBuilder.Entity<UserUsedLimit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_UserUsedLimit_PK_UserUsedLimit");

            entity.ToTable("UserUsedLimit");

            entity.HasIndex(e => e.Tstamp, "public_UserUsedLimit_INDEX_VV_19190_19189");

            entity.HasIndex(e => e.UserId, "public_UserUsedLimit_IX_UserUsedLimit");

            entity.HasIndex(e => e.UserId, "public_UserUsedLimit_IX_UserUsedLimit_UserID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UsedAmount).HasPrecision(18, 2);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.UserType).HasMaxLength(10);
        });

        modelBuilder.Entity<VStudentInfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_StudentInfo");

            entity.Property(e => e.NTerm)
                .HasMaxLength(20)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastname)
                .HasColumnType("character varying")
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<VUserList>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_UserList");

            entity.Property(e => e.CDel)
                .HasColumnType("character varying")
                .HasColumnName("cDel");
            entity.Property(e => e.CType).HasColumnName("cType");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NMoney).HasColumnName("nMoney");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SLastName)
                .HasColumnType("character varying")
                .HasColumnName("sLastName");
            entity.Property(e => e.SName)
                .HasColumnType("character varying")
                .HasColumnName("sName");
            entity.Property(e => e.SStudentId)
                .HasColumnType("character varying")
                .HasColumnName("sStudentID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<VgetAllStudentInfoByRoom>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VGetAllStudentInfoByRoom");

            entity.Property(e => e.AddressLat).HasColumnName("addressLat");
            entity.Property(e => e.AddressLng).HasColumnName("addressLng");
            entity.Property(e => e.BaseSalary).HasColumnName("baseSalary");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CSex)
                .HasMaxLength(1)
                .HasColumnName("cSex");
            entity.Property(e => e.CSms)
                .HasMaxLength(1)
                .HasColumnName("cSMS");
            entity.Property(e => e.CTelSms)
                .HasMaxLength(50)
                .HasColumnName("cTelSMS");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.ContactPeak).HasMaxLength(100);
            entity.Property(e => e.DBirth)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dBirth");
            entity.Property(e => e.DPicUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dPicUpdate");
            entity.Property(e => e.DUpdate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dUpdate");
            entity.Property(e => e.Height).HasPrecision(6, 2);
            entity.Property(e => e.MoveInDate)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("moveInDate");
            entity.Property(e => e.MoveOutReason)
                .HasMaxLength(200)
                .HasColumnName("moveOutReason");
            entity.Property(e => e.NHealthId).HasColumnName("nHealthID");
            entity.Property(e => e.NMax)
                .HasPrecision(18, 2)
                .HasColumnName("nMax");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 2)
                .HasColumnName("nMoney");
            entity.Property(e => e.NMonth).HasColumnName("nMonth");
            entity.Property(e => e.NPicversion).HasColumnName("nPicversion");
            entity.Property(e => e.NSonNumber).HasColumnName("nSonNumber");
            entity.Property(e => e.NStudentNumber).HasColumnName("nStudentNumber");
            entity.Property(e => e.NStudentStatus).HasColumnName("nStudentStatus");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.Note).HasMaxLength(250);
            entity.Property(e => e.OldSchoolAumpher)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolAumpher");
            entity.Property(e => e.OldSchoolGpa)
                .HasPrecision(18, 2)
                .HasColumnName("oldSchoolGPA");
            entity.Property(e => e.OldSchoolGraduated)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolGraduated");
            entity.Property(e => e.OldSchoolName)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolName");
            entity.Property(e => e.OldSchoolProvince)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolProvince");
            entity.Property(e => e.OldSchoolTumbon)
                .HasMaxLength(250)
                .HasColumnName("oldSchoolTumbon");
            entity.Property(e => e.SAddress)
                .HasMaxLength(512)
                .HasColumnName("sAddress");
            entity.Property(e => e.SBlood)
                .HasMaxLength(200)
                .HasColumnName("sBlood");
            entity.Property(e => e.SCity)
                .HasMaxLength(512)
                .HasColumnName("sCity");
            entity.Property(e => e.SCountry)
                .HasMaxLength(512)
                .HasColumnName("sCountry");
            entity.Property(e => e.SDeleted)
                .HasMaxLength(10)
                .HasColumnName("sDeleted");
            entity.Property(e => e.SEmail)
                .HasMaxLength(256)
                .HasColumnName("sEmail");
            entity.Property(e => e.SFinger).HasColumnName("sFinger");
            entity.Property(e => e.SFinger2).HasColumnName("sFinger2");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SIdentification)
                .HasMaxLength(13)
                .HasColumnName("sIdentification");
            entity.Property(e => e.SLastname)
                .HasMaxLength(256)
                .HasColumnName("sLastname");
            entity.Property(e => e.SName)
                .HasMaxLength(256)
                .HasColumnName("sName");
            entity.Property(e => e.SNickName)
                .HasMaxLength(256)
                .HasColumnName("sNickName");
            entity.Property(e => e.SNickNameEn)
                .HasMaxLength(256)
                .HasColumnName("sNickNameEN");
            entity.Property(e => e.SPassword)
                .HasMaxLength(100)
                .HasColumnName("sPassword");
            entity.Property(e => e.SPhone)
                .HasMaxLength(100)
                .HasColumnName("sPhone");
            entity.Property(e => e.SPostalcode)
                .HasMaxLength(10)
                .HasColumnName("sPostalcode");
            entity.Property(e => e.SSickDanger)
                .HasMaxLength(250)
                .HasColumnName("sSickDanger");
            entity.Property(e => e.SSickDrug)
                .HasMaxLength(250)
                .HasColumnName("sSickDrug");
            entity.Property(e => e.SSickFood)
                .HasMaxLength(250)
                .HasColumnName("sSickFood");
            entity.Property(e => e.SSickNormal)
                .HasMaxLength(250)
                .HasColumnName("sSickNormal");
            entity.Property(e => e.SSickOther)
                .HasMaxLength(250)
                .HasColumnName("sSickOther");
            entity.Property(e => e.SStudentAumpher)
                .HasMaxLength(50)
                .HasColumnName("sStudentAumpher");
            entity.Property(e => e.SStudentHomeNumber)
                .HasMaxLength(100)
                .HasColumnName("sStudentHomeNumber");
            entity.Property(e => e.SStudentHomeRegisterCode)
                .HasMaxLength(20)
                .HasColumnName("sStudentHomeRegisterCode");
            entity.Property(e => e.SStudentHousePhone)
                .HasMaxLength(50)
                .HasColumnName("sStudentHousePhone");
            entity.Property(e => e.SStudentId)
                .HasMaxLength(20)
                .HasColumnName("sStudentID");
            entity.Property(e => e.SStudentIdCardNumber)
                .HasMaxLength(20)
                .HasColumnName("sStudentIdCardNumber");
            entity.Property(e => e.SStudentLastEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastEN");
            entity.Property(e => e.SStudentLastOther)
                .HasMaxLength(50)
                .HasColumnName("sStudentLastOther");
            entity.Property(e => e.SStudentMuu)
                .HasMaxLength(50)
                .HasColumnName("sStudentMuu");
            entity.Property(e => e.SStudentNameEn)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameEN");
            entity.Property(e => e.SStudentNameOther)
                .HasMaxLength(50)
                .HasColumnName("sStudentNameOther");
            entity.Property(e => e.SStudentNation)
                .HasMaxLength(50)
                .HasColumnName("sStudentNation");
            entity.Property(e => e.SStudentPicture)
                .HasMaxLength(255)
                .HasColumnName("sStudentPicture");
            entity.Property(e => e.SStudentPost)
                .HasMaxLength(50)
                .HasColumnName("sStudentPost");
            entity.Property(e => e.SStudentProvince)
                .HasMaxLength(50)
                .HasColumnName("sStudentProvince");
            entity.Property(e => e.SStudentRace)
                .HasMaxLength(50)
                .HasColumnName("sStudentRace");
            entity.Property(e => e.SStudentReligion)
                .HasMaxLength(50)
                .HasColumnName("sStudentReligion");
            entity.Property(e => e.SStudentRoad)
                .HasMaxLength(50)
                .HasColumnName("sStudentRoad");
            entity.Property(e => e.SStudentSoy)
                .HasMaxLength(50)
                .HasColumnName("sStudentSoy");
            entity.Property(e => e.SStudentTitle)
                .HasMaxLength(50)
                .HasColumnName("sStudentTitle");
            entity.Property(e => e.SStudentTumbon)
                .HasMaxLength(50)
                .HasColumnName("sStudentTumbon");
            entity.Property(e => e.SSubtopic)
                .HasMaxLength(512)
                .HasColumnName("sSubtopic");
            entity.Property(e => e.SToken).HasColumnName("sToken");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Weight).HasPrecision(6, 2);
        });

        modelBuilder.Entity<VgetPlanRoomList>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VGetPlanRoomList");

            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTsubLevel2)
                .HasMaxLength(100)
                .HasColumnName("nTSubLevel2");
            entity.Property(e => e.PlanName).HasMaxLength(250);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SubLevel).HasMaxLength(20);
        });

        modelBuilder.Entity<ViewActiveSchoolList>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("View_ActiveSchoolList", "masterdb_schema");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.NCompany).HasColumnName("nCompany");
            entity.Property(e => e.SCompany)
                .HasMaxLength(250)
                .HasColumnName("sCompany");
        });

        modelBuilder.Entity<ViewTopupMemoryDataNew>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_TopupMemoryDataNew");

            entity.Property(e => e.Keyword)
                .HasColumnType("character varying")
                .HasColumnName("KEYWORD");
            entity.Property(e => e.KeywordOrder).HasColumnName("KEYWORD_ORDER");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId)
                .HasColumnType("character varying")
                .HasColumnName("User_Id");
            entity.Property(e => e.UserName).HasColumnName("User_Name");
            entity.Property(e => e.UserType).HasColumnName("User_Type");
        });

        modelBuilder.Entity<ViewTopupMemoryDataOptimized>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_TopupMemoryDataOptimized");

            entity.Property(e => e.Keyword)
                .HasColumnType("character varying")
                .HasColumnName("KEYWORD");
            entity.Property(e => e.KeywordOrder).HasColumnName("KEYWORD_ORDER");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId)
                .HasColumnType("character varying")
                .HasColumnName("User_Id");
            entity.Property(e => e.UserName).HasColumnName("User_Name");
            entity.Property(e => e.UserType).HasColumnName("User_Type");
        });

        modelBuilder.Entity<ViewTopupMemoryDataOptimized1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_TopupMemoryDataOptimized1");

            entity.Property(e => e.Keyword)
                .HasColumnType("character varying")
                .HasColumnName("KEYWORD");
            entity.Property(e => e.KeywordOrder).HasColumnName("KEYWORD_ORDER");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId)
                .HasColumnType("character varying")
                .HasColumnName("User_Id");
            entity.Property(e => e.UserName).HasColumnName("User_Name");
            entity.Property(e => e.UserType).HasColumnName("User_Type");
        });

        modelBuilder.Entity<ViewTopupMemoryDataRedi>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_TopupMemoryData_Redis");

            entity.Property(e => e.Keyword).HasColumnName("KEYWORD");
            entity.Property(e => e.KeywordOrder).HasColumnName("KEYWORD_ORDER");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId)
                .HasColumnType("character varying")
                .HasColumnName("User_Id");
            entity.Property(e => e.UserName).HasColumnName("User_Name");
            entity.Property(e => e.UserType).HasColumnName("User_Type");
        });

        modelBuilder.Entity<ViewTopupMemoryDataSyncQueue>(entity =>
        {
            entity.HasKey(e => e.QueueId).HasName("ViewTopupMemoryDataSyncQueue_pkey");

            entity.ToTable("ViewTopupMemoryDataSyncQueue");

            entity.HasIndex(e => new { e.SchoolId, e.UserId }, "idx_syncqueueviewmemory_school_sid_unprocessed")
                .HasFilter("(\"Processed\" = false)")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.CreatedAt, "idx_viewtopupmemorydatasyncqueue_createdat_covering");

            entity.Property(e => e.QueueId).HasColumnName("QueueID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Processed).HasDefaultValue(false);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId).HasColumnName("User_Id");
        });

        modelBuilder.Entity<ViewTopupMemoryDatum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_TopupMemoryData");

            entity.Property(e => e.Keyword)
                .HasColumnType("character varying")
                .HasColumnName("KEYWORD");
            entity.Property(e => e.KeywordOrder).HasColumnName("KEYWORD_ORDER");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId)
                .HasColumnType("character varying")
                .HasColumnName("User_Id");
            entity.Property(e => e.UserName).HasColumnName("User_Name");
            entity.Property(e => e.UserType).HasColumnName("User_Type");
        });

        modelBuilder.Entity<VreturnedTempCard>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VReturnedTempCard");

            entity.Property(e => e.CardId).HasColumnName("CardID");
        });
        modelBuilder.HasSequence("ParentCodeSequence")
            .StartsAt(13520015860L)
            .HasMin(-9223372036854775808L)
            .HasMax(99999999999L)
            .IsCyclic();
        modelBuilder.HasSequence("rule_id_seq", "rds_pg_sql_ccl");
        modelBuilder.HasSequence("SchoolEmployeeSyncQueue_QueueID_seq").IsCyclic();
        modelBuilder.HasSequence("SchoolUserSyncQueue_QueueID_seq").IsCyclic();
        modelBuilder.HasSequence("ViewTopupMemoryDataSyncQueue_QueueID_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
