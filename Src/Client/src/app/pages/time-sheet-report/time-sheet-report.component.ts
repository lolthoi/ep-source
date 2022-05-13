import { ProjectModel } from './../../shared/models/project.model';
import { ProjectService } from './../../shared/services/project.service';
import { FilterParamUserActivity } from './../../shared/models/time-sheet-report-user-activity.model';
import { ActivityGroupUserService } from './../../shared/services/activity-group-user.service';
import { TimeSheetReportProjectService } from './../../shared/services/time-sheet-report-project.service';
import { FilterParamUserRecord } from './../../shared/models/filter-param-user-record.model';
import { TimeSheetReportUserService } from './../../shared/services/time-sheet-report-user.service';
import { Component, NgModule, OnInit, ViewChild } from '@angular/core';
import {
  DxFormModule,
  DxTabPanelModule,
  DxSelectBoxModule,
  DxDataGridModule,
  DxButtonModule,
  DxTagBoxModule,
  DxPopupModule,
  DxDateBoxModule,
  DxDropDownBoxModule,
} from 'devextreme-angular';
import { UserModel } from '../../shared/models/user.model';
import { ActivityGroupModel } from '../../shared/models/activity-group.model';
import { UserService } from '../../shared/services/user.service';
import { TimeSheetReportDetailRouterModel } from '../../shared/models/time-sheet-report-detail-router.model';
import { ActivityGroupService } from '../../shared/services/activity-group.service';
import { TimeSheetReportDetailService } from '../../shared/services/time-sheet-report-detail.service';
import { TimeSheetReportDetailModel } from '../../shared/models/time-sheet-report-detail-model';
import { AuthService } from '../../shared/services/auth.service';
import { PaginationModel } from '../../shared/models/pagination.model';
import { BrowserModule } from '@angular/platform-browser';
import { NgxPaginationModule } from 'ngx-pagination';
import { TimeSheetRecordService } from '../../shared/services/time-sheet-record.service';
import { CommonService } from '../../shared/services/common.service';
import {
  TimeSheetReportFormComponent,
  TimeSheetReportFormModel,
  TimeSheetReportFormModule,
  TimeSheetReportFormState,
} from '../../shared/components/time-sheet-report-form/time-sheet-report-form.component';
import { FilterParamProjectRecord } from 'src/app/shared/models/filter-param-project-record.model';
import { ActivityRecordViewModel } from 'src/app/shared/models/time-sheet-record.model';
import { FilterParamProjectActivity } from 'src/app/shared/models/time-sheet-report-project-activity.model';

@Component({
  selector: 'app-time-sheet-report',
  templateUrl: './time-sheet-report.component.html',
  styleUrls: ['./time-sheet-report.component.scss'],
})
export class TimeSheetReportComponent implements OnInit {
  currentTimeSheetRecord: TimeSheetReportFormModel =
    new TimeSheetReportFormModel();

  selectedIndex: number = 0;
  listUser: UserModel[] = [];
  listTeam: ProjectModel[] = [];
  listGroup: ActivityGroupModel[] = [];
  listManagerGroup: ActivityGroupModel[] = [];
  listActivity: ActivityRecordViewModel[];
  listManagerActivity: ActivityRecordViewModel[];

  masterDataUser: UserModel[] = [];
  masterDataTeam: ProjectModel[] = [];
  masterDataGroup: ActivityGroupModel[] = [];
  masterDataActivity: ActivityRecordViewModel[];

  timeSheetReportDetailRouterModel = new TimeSheetReportDetailRouterModel();
  totalTime: string;
  isAdminRole = false;
  isShowEdit = false;

  timeSheetReportDetailModelSelected: TimeSheetReportDetailModel;

  page = 1;
  count = 0;
  pageSize = 50;
  pageSizes = [50, 100, 150];
  popupDeleteTimeSheetRecord = false;

  gridColumns = [
    'timeSheetRecordName',
    'nameTask',
    'fullName',
    'startDate',
    'endDate',
    'duration',
  ];

  userTimeRecord: any;
  now: Date = new Date();
  userParams = new FilterParamUserRecord();
  userActivityParams = new FilterParamUserActivity();
  currentYearUser: number;
  YearDataSourceUser = [
    { id: 1, value: this.now.getFullYear() },
    { id: 2, value: this.now.getFullYear() - 1 },
    { id: 3, value: this.now.getFullYear() - 2 },
    { id: 4, value: this.now.getFullYear() - 3 },
    { id: 5, value: this.now.getFullYear() - 4 },
  ];

  projectTimeRecord: any;
  projectParams = new FilterParamProjectRecord();
  projectActivityParams = new FilterParamProjectActivity();
  currentYearProject: number;
  YearDataSourceProject = [
    { id: 1, value: this.now.getFullYear() },
    { id: 2, value: this.now.getFullYear() - 1 },
    { id: 3, value: this.now.getFullYear() - 2 },
  ];

  projectShow = true;
  isManager = true;
  isQa = true;
  isUser = false;
  user: any;
  manager: any;
  detailCol: number = 9;
  userCol: number = 3;
  dateCol: number = 2;
  selectedRowKeysUserFilter: any;

  pagingModel: PaginationModel<TimeSheetReportDetailModel> =
    new PaginationModel<TimeSheetReportDetailModel>();

  @ViewChild(TimeSheetReportFormComponent)
  timeSheetReportFormComponent: TimeSheetReportFormComponent;

  userActivityReport: any;
  projectActivityReport: any;
  groupIds: string[] = [];
  projectIds: number[] = [];

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private projectService: ProjectService,
    private activityGroupService: ActivityGroupService,
    private timeSheetRecordService: TimeSheetRecordService,
    private timeSheetReportDetailService: TimeSheetReportDetailService,
    private timeSheetReportUserService: TimeSheetReportUserService,
    private timeSheetReportProjectService: TimeSheetReportProjectService,
    private activityGroupUserService: ActivityGroupUserService,
    private commonService: CommonService
  ) {
    this.isAdminRole = this.authService.isRoleAdministrator;
    this.user = this.authService.getUser;
  }
  ngOnInit() {
    this.onInit();
    this.checkManager();
    this.InitUserReport();
    this.InitDetailReport();
  }

  checkManager() {
    if (this.isAdminRole == false) {
      this.activityGroupUserService
        .CheckManagerByUserId(this.user.id)
        .subscribe((res) => {
          this.manager = res;
          if (this.manager == 0) {
            this.isUser = true;
            this.projectShow = false;
            this.isManager = false;
            this.isQa = false;
            this.selectedIndex = 2;
            this.detailCol = 7;
            this.dateCol = 1;
            this.selectedRowKeysUserFilter = this.user.email;
          }
          if (this.manager == 2) {
            this.isManager = false;
            this.InitProjectRecord();
          } else {
            this.InitProjectRecord();
            this.InitUserActivityReport();
            this.InitProjectActivityReport();
          }
        });
    } else {
      this.InitProjectRecord();
      this.InitUserActivityReport();
      this.InitProjectActivityReport();
    }
  }
  refreshDataGrid() {
    this.InitUserReport();
    this.InitDetailReport();
    this.InitProjectRecord();
    this.InitUserActivityReport();
    this.InitProjectActivityReport();
  }
  onInit() {
    this.projectService.getTeamForReport().subscribe(
      (res: ProjectModel[]) => {
        this.listTeam = res;
        this.masterDataTeam = Object.assign(this.listTeam, []);
      },
      (err) => {
        console.log(err);
      }
    );
    this.userService.getUsersForTimeSheetReport(this.projectIds).subscribe(
      (response: UserModel[]) => {
        this.listUser = response;
        this.masterDataUser = Object.assign(this.listUser, []);
      },
      (error) => {
        console.log(error);
      }
    );
    this.activityGroupService.ActivityGroupTimeSheetReport().subscribe(
      (response: ActivityGroupModel[]) => {
        this.listGroup = response;
        this.masterDataGroup = Object.assign(this.listGroup, []);
      },
      (error) => {
        console.log(error);
      }
    );
    this.activityGroupService.ActivityGroupProjectReport().subscribe(
      (res: ActivityGroupModel[]) => {
        this.listManagerGroup = res;
      },
      (err) => {}
    );
    this.timeSheetReportDetailService
      .getListActivityName(this.groupIds)
      .subscribe(
        (response: ActivityRecordViewModel[]) => {
          this.listActivity = response;
          this.masterDataActivity = Object.assign(this.listActivity, []);
        },
        (error) => {
          console.log(error);
        }
      );
    this.timeSheetReportDetailService
      .getActivityForReportProject(this.groupIds)
      .subscribe(
        (response: ActivityRecordViewModel[]) => {
          this.listManagerActivity = response;
        },
        (error) => {
          console.log(error);
        }
      );
  }

  // Report Project by month
  InitProjectRecord() {
    this.projectParams.year = this.now.getFullYear();
    this.currentYearProject = this.YearDataSourceProject[0].id;
    this.timeSheetReportProjectService.GetAll(this.projectParams).subscribe(
      (res) => {
        this.projectTimeRecord = res;
      },
      (err) => {
        console.log(err);
      }
    );
  }
  changeYearProject(e: any) {
    this.projectParams.year = e.selectedItem.value;
  }
  onSelectFilterProject(e: any) {
    this.projectParams.gIds = [];
    e.selectedRowsData.forEach((e) => {
      this.projectParams.gIds.push(e.id);
    });
  }
  OnFilterProject() {
    this.timeSheetReportProjectService.GetAll(this.projectParams).subscribe(
      (res) => {
        this.projectTimeRecord = res;
      },
      (err) => {}
    );
  }
  FilterProjectReport = {
    text: 'Apply filter',
    width: 'auto',
    type: 'success',
    onClick: this.OnFilterProject.bind(this),
  };

  //Report Project Activity
  InitProjectActivityReport() {
    var date = new Date();
    this.projectActivityParams.startDate = new Date(
      date.getFullYear(),
      date.getMonth(),
      1
    );

    this.projectActivityParams.endDate = new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      23,
      59,
      0
    );
    let entity = Object.assign({}, this.projectActivityParams);
    entity.startDate = this.commonService.convertSameDateTimeToDateUtc(
      this.projectActivityParams.startDate
    );
    entity.endDate = this.commonService.convertSameDateTimeToDateUtc(
      this.projectActivityParams.endDate
    );
    this.timeSheetReportProjectService.GetAllProjectActivity(entity).subscribe(
      (res) => {
        this.projectActivityReport = res;
      },
      (err) => {}
    );
  }
  onSelectFilterProjectActivity(e: any) {
    this.projectActivityParams.groupIds = [];
    e.selectedRowsData.forEach((e) => {
      this.projectActivityParams.groupIds.push(e.id);
    });
    this.timeSheetReportDetailService
      .getActivityForReportProject(this.projectActivityParams.groupIds)
      .subscribe(
        (response: ActivityRecordViewModel[]) => {
          this.listManagerActivity = response;
        },
        (error) => {
          console.log(error);
        }
      );
  }
  onSelectTaskProjectActivity(e: any) {
    this.projectActivityParams.activityIds = [];
    e.selectedRowsData.forEach((e) => {
      this.projectActivityParams.activityIds.push(e.id);
    });
  }
  onChangedStartDateProjectActivity(e: any) {
    if (e.value === null) {
      this.projectActivityParams.startDate = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
  }
  onChangedEndDateProjectActivity(e: any) {
    if (e.value === null) {
      this.projectActivityParams.endDate = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
  }
  FilterProjectActivityReport = {
    text: 'Apply filter',
    width: 'auto',
    type: 'success',
    onClick: this.OnFilterProjectActivity.bind(this),
  };
  OnFilterProjectActivity() {
    let entity = Object.assign({}, this.projectActivityParams);
    entity.startDate = this.commonService.convertSameDateTimeToDateUtc(
      this.projectActivityParams.startDate
    );
    entity.endDate = this.commonService.convertSameDateTimeToDateUtc(
      this.projectActivityParams.endDate
    );

    this.timeSheetReportProjectService.GetAllProjectActivity(entity).subscribe(
      (res) => {
        this.projectActivityReport = res;
      },
      (err) => {}
    );
  }

  //Report User by month
  InitUserReport() {
    this.userParams.year = this.now.getFullYear();
    this.currentYearUser = this.YearDataSourceUser[0].id;
    this.timeSheetReportUserService.GetAll(this.userParams).subscribe(
      (res) => {
        this.userTimeRecord = res;
      },
      (err) => {
        console.log(err);
      }
    );
  }
  changeYearUser(e: any) {
    this.userParams.year = e.selectedItem.value;
  }
  onSelectTeamUser(e: any) {
    this.userParams.pIds = [];
    e.selectedRowsData.forEach((e) => {
      this.userParams.pIds.push(e.id);
    });
    this.userService.getUsersForTimeSheetReport(this.userParams.pIds).subscribe(
      (response: UserModel[]) => {
        this.listUser = response;
      },
      (error) => {
        console.log(error);
      }
    );
  }
  onSelectUserUser(e: any) {
    this.userParams.uIds = [];
    e.selectedRowsData.forEach((e) => {
      this.userParams.uIds.push(e.id);
    });
  }
  OnFilterUser() {
    this.timeSheetReportUserService.GetAll(this.userParams).subscribe(
      (res) => {
        this.userTimeRecord = res;
      },
      (err) => {}
    );
  }
  FilterUserReport = {
    text: 'Apply filter',
    width: 'auto',
    type: 'success',
    onClick: this.OnFilterUser.bind(this),
  };

  //Report User Activity
  InitUserActivityReport() {
    var date = new Date();
    this.userActivityParams.startDate = new Date(
      date.getFullYear(),
      date.getMonth(),
      1
    );

    this.userActivityParams.endDate = new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      23,
      59,
      0
    );
    let entity = Object.assign({}, this.userActivityParams);
    entity.startDate = this.commonService.convertSameDateTimeToDateUtc(
      this.userActivityParams.startDate
    );
    entity.endDate = this.commonService.convertSameDateTimeToDateUtc(
      this.userActivityParams.endDate
    );
    this.timeSheetReportUserService.GetAllUserActivity(entity).subscribe(
      (res) => {
        this.userActivityReport = res;
      },
      (err) => {}
    );
  }
  onSelectTeamUserActivity(e: any) {
    this.userActivityParams.pIds = [];
    e.selectedRowsData.forEach((e) => {
      this.userActivityParams.pIds.push(e.id);
    });
    this.userService
      .getUsersForTimeSheetReport(this.userActivityParams.pIds)
      .subscribe(
        (response: UserModel[]) => {
          this.listUser = response;
        },
        (error) => {
          console.log(error);
        }
      );
  }
  onSelectUserUserActivity(e: any) {
    this.userActivityParams.uIds = [];
    e.selectedRowsData.forEach((e) => {
      this.userActivityParams.uIds.push(e.id);
    });
  }
  onSelectActivityUserActivity(e: any) {
    this.userActivityParams.activityIds = [];
    e.selectedRowsData.forEach((e) => {
      this.userActivityParams.activityIds.push(e.id);
    });
  }
  onChangedStartDateUserActivity(e: any) {
    if (e.value === null) {
      this.userActivityParams.startDate = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
  }
  onChangedEndDateUserActivity(e: any) {
    if (e.value === null) {
      this.userActivityParams.endDate = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
  }
  FilterUserActivityReport = {
    text: 'Apply filter',
    width: 'auto',
    type: 'success',
    onClick: this.OnFilterUserActivity.bind(this),
  };
  OnFilterUserActivity() {
    let entity = Object.assign({}, this.userActivityParams);
    entity.startDate = this.commonService.convertSameDateTimeToDateUtc(
      this.userActivityParams.startDate
    );
    entity.endDate = this.commonService.convertSameDateTimeToDateUtc(
      this.userActivityParams.endDate
    );

    this.timeSheetReportUserService.GetAllUserActivity(entity).subscribe(
      (res) => {
        this.userActivityReport = res;
      },
      (err) => {}
    );
  }

  //Report Detail
  InitDetailReport() {
    var date = new Date();
    this.timeSheetReportDetailRouterModel.page = 1;
    this.timeSheetReportDetailRouterModel.pageSize = 50;
    this.timeSheetReportDetailRouterModel.startDate = new Date(
      date.getFullYear(),
      date.getMonth(),
      1
    );

    this.timeSheetReportDetailRouterModel.endDate = new Date(
      date.getFullYear(),
      date.getMonth() + 1,
      0
    );

    this.timeSheetReportDetailService
      .getAllRecordPaging(this.timeSheetReportDetailRouterModel)
      .subscribe(
        (response: PaginationModel<TimeSheetReportDetailModel>) => {
          this.pagingModel = response;
          this.page = response.page;
          this.count = response.totalCount;
        },
        (error) => {
          console.log(error);
        }
      );
  }
  onSelectTeamUserDetail(e: any) {
    this.timeSheetReportDetailRouterModel.projectIds = [];
    e.selectedRowsData.forEach((e) => {
      this.timeSheetReportDetailRouterModel.projectIds.push(e.id);
    });

    this.userService
      .getUsersForTimeSheetReport(
        this.timeSheetReportDetailRouterModel.projectIds
      )
      .subscribe(
        (response: UserModel[]) => {
          this.listUser = response;
        },
        (error) => {
          console.log(error);
        }
      );
  }
  onSelectUserDetail(e: any) {
    this.timeSheetReportDetailRouterModel.userIds = [];
    e.selectedRowsData.forEach((e) => {
      this.timeSheetReportDetailRouterModel.userIds.push(e.id);
    });
  }
  onSelectGroupDetail(e: any) {
    this.timeSheetReportDetailRouterModel.tSAcitivityGroupIds = [];
    e.selectedRowsData.forEach((e) => {
      this.timeSheetReportDetailRouterModel.tSAcitivityGroupIds.push(e.id);
    });
    this.timeSheetReportDetailService
      .getListActivityName(
        this.timeSheetReportDetailRouterModel.tSAcitivityGroupIds
      )
      .subscribe(
        (response: ActivityRecordViewModel[]) => {
          this.listActivity = response;
        },
        (error) => {
          console.log(error);
        }
      );
  }
  onSelectActivityDetail(e: any) {
    this.timeSheetReportDetailRouterModel.taskIds = [];
    e.selectedRowsData.forEach((e) => {
      this.timeSheetReportDetailRouterModel.taskIds.push(e.id);
    });
  }
  onValueChangedStartDate(e: any) {
    this.timeSheetReportDetailRouterModel.startDate = e.value;
  }
  onValueChangedEndDate(e: any) {
    this.timeSheetReportDetailRouterModel.endDate = e.value;
  }
  OnFilterDetail() {
    this.timeSheetReportDetailService
      .getAllRecordPaging(this.timeSheetReportDetailRouterModel)
      .subscribe(
        (response: PaginationModel<TimeSheetReportDetailModel>) => {
          this.pagingModel = response;
          this.page = response.page;
          this.count = response.totalCount;
        },
        (error) => {
          console.log(error);
        }
      );
  }
  FilterDetailReport = {
    text: 'Apply filter',
    width: 'auto',
    type: 'success',
    onClick: this.OnFilterDetail.bind(this),
  };

  getString(number) {
    return number.toString().padStart(2, '0');
  }

  addNewTimeSheetRecordByAdmin(e) {
    this.currentTimeSheetRecord.state = TimeSheetReportFormState.CREATE;
    this.currentTimeSheetRecord.data = new TimeSheetReportDetailModel();
    this.currentTimeSheetRecord.data.timeSheetRecordName = '';
    this.currentTimeSheetRecord.data.timeSheetRecordId = '';
    this.timeSheetReportFormComponent.open();
  }

  updateTimeSheetRecordByAdmin(e, data) {
    this.currentTimeSheetRecord.state = TimeSheetReportFormState.EDIT;
    this.currentTimeSheetRecord.data = new TimeSheetReportDetailModel(data);
    this.timeSheetReportFormComponent.open();
  }

  deleteTimeSheetByAdmin(e, data) {
    this.popupDeleteTimeSheetRecord = true;
    this.timeSheetReportDetailModelSelected = new TimeSheetReportDetailModel(
      data
    );
  }

  confirmDeletePopupTimeSheetRecordButtonOptions = {
    icon: 'save',
    text: 'Ok',
    onClick: (e) => {
      this.timeSheetRecordService
        .delete(this.timeSheetReportDetailModelSelected.timeSheetRecordId)
        .subscribe(
          () => {
            this.commonService.UI.multipleNotify(
              'Time record has been deleted.',
              'success',
              2000
            );
            this.timeSheetReportDetailRouterModel.page = this.page;
            this.timeSheetReportDetailRouterModel.pageSize = this.pageSize;
            this.refreshDataGrid();
            this.popupDeleteTimeSheetRecord = false;
            this.timeSheetReportDetailModelSelected = null;
          },
          (error) => {
            this.commonService.UI.multipleNotify(error.error, 'error', 5000);
            this.popupDeleteTimeSheetRecord = false;
            this.timeSheetReportDetailModelSelected = null;
          }
        );
    },
  };

  closeDeletePopupTimeSheetRecordButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupDeleteTimeSheetRecord = false;
      this.timeSheetReportDetailModelSelected = null;
    },
  };

  //Handle paginator event
  handlePageChange(event) {
    this.page = event;
    this.timeSheetReportDetailRouterModel.page = this.page;
    this.OnFilterDetail();
  }
  handlePageSizeChange(e: any) {
    this.pageSize = e.target.value;
    this.page = 1;
    this.timeSheetReportDetailRouterModel.page = this.page;
    this.timeSheetReportDetailRouterModel.pageSize = this.pageSize;
    this.OnFilterDetail();
  }
}

@NgModule({
  imports: [
    BrowserModule,
    DxFormModule,
    DxTabPanelModule,
    DxSelectBoxModule,
    DxDataGridModule,
    DxButtonModule,
    NgxPaginationModule,
    DxTagBoxModule,
    DxPopupModule,
    DxDropDownBoxModule,
    TimeSheetReportFormModule,
    DxSelectBoxModule,
    DxDateBoxModule,
  ],
  declarations: [TimeSheetReportComponent],
  bootstrap: [TimeSheetReportComponent],
})
export class TimeSheetReportModule {}
