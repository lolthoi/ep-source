import { TimeSheetReportComponent } from './../../../pages/time-sheet-report/time-sheet-report.component';
import { TimeSheetReportDetailService } from './../../services/time-sheet-report-detail.service';
import {
  EventEmitter,
  Input,
  NgModule,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { Component } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {
  DxButtonModule,
  DxFormComponent,
  DxFormModule,
  DxPopupModule,
  DxScrollViewModule,
  DxSelectBoxModule,
  DxValidatorModule,
  DxValidationSummaryModule,
  DxTextBoxModule,
  DxDateBoxModule,
} from 'devextreme-angular';
import { TimeSheetRecordModel } from '../../models/time-sheet-record.model';
import { TimeSheetReportDetailModel } from '../../models/time-sheet-report-detail-model';
import { AuthService } from '../../services/auth.service';
import { CommonService } from '../../services/common.service';
import { TimeSheetRecordService } from '../../services/time-sheet-record.service';

@Component({
  selector: 'app-time-sheet-report-form',
  templateUrl: './time-sheet-report-form.component.html',
  styleUrls: ['./time-sheet-report-form.component.scss'],
})
export class TimeSheetReportFormComponent implements OnInit {
  timeSheetRecordModel: TimeSheetRecordModel = new TimeSheetRecordModel();
  @Input() model: TimeSheetReportFormModel;
  @Input() listUser: [];

  @Output() onRefreshGrid = new EventEmitter();
  popupVisible = false;
  popupTitle = '';
  formState = TimeSheetReportFormState;

  currTimeSheetReportDetailModel: TimeSheetReportDetailModel =
    new TimeSheetReportDetailModel();
  @ViewChild(DxFormComponent, { static: false }) myForm: DxFormComponent;

  selectedUser: any;
  selectedActivity: any;
  userActivity: any;
  choosenUser = true;
  chosenDate = new Date();
  dateboxStartTimeComp: any;
  dateboxEndTimeComp: any;

  constructor(
    private authService: AuthService,
    private timeSheetRecordService: TimeSheetRecordService,
    private commonService: CommonService,
    private TimeSheetReportDetailService: TimeSheetReportDetailService
  ) {}
  ngOnInit() {
    this.chosenDate = new Date();
    this.chosenDate.setSeconds(0);
    this.chosenDate.setMilliseconds(0);
  }
  onInitStartTimeComp(e) {
    this.dateboxStartTimeComp = e.component;
  }

  onInitEndTimeComp(e) {
    this.dateboxEndTimeComp = e.component;
  }

  open() {
    switch (this.model.state) {
      case TimeSheetReportFormState.CREATE:
        this.popupTitle = 'CREATE TIMESHEET RECORD';
        if (this.myForm) {
          this.myForm.instance.resetValues();
        }
        this.currTimeSheetReportDetailModel.backlogId = null;
        this.currTimeSheetReportDetailModel.taskId = null;
        this.currTimeSheetReportDetailModel.startDate = new Date();
        this.currTimeSheetReportDetailModel.startDate.setSeconds(0);
        this.currTimeSheetReportDetailModel.startDate.setMilliseconds(0);
        this.currTimeSheetReportDetailModel.endDate = new Date();
        this.currTimeSheetReportDetailModel.endDate.setSeconds(0);
        this.currTimeSheetReportDetailModel.endDate.setMilliseconds(0);
        break;
      case TimeSheetReportFormState.EDIT:
        this.popupTitle = 'EDIT TIMESHEET RECORD';
        this.currTimeSheetReportDetailModel = this.model.data;
        this.currTimeSheetReportDetailModel.startDate =
          this.timeSheetRecordService.parseStringTimeZoneToDate(
            this.model.data.startDate
          );
        this.currTimeSheetReportDetailModel.endDate =
          this.timeSheetRecordService.parseStringTimeZoneToDate(
            this.model.data.endDate
          );
        this.InitTaskUser(this.currTimeSheetReportDetailModel.userId);
        break;
    }
    this.popupVisible = true;
  }

  closeButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupVisible = false;
      this.selectedUser = null;
      this.selectedActivity = null;
      this.choosenUser = true;
    },
  };

  InitTaskUser(userId: number) {
    this.TimeSheetReportDetailService.getActivityByUserId(userId).subscribe(
      (res) => {
        this.userActivity = res;
        this.choosenUser = false;
      },
      (err) => {}
    );
  }

  onValueChangeUser(e: any) {
    if (e.value !== null) {
      this.InitTaskUser(e.value);
    } else {
      this.choosenUser = true;
    }
  }
  onValueChangeActivity(e: any) {}
  onValueChangedStartTime(e: any) {
    if (e.value === null) {
      this.currTimeSheetReportDetailModel.startDate = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
    if (
      this.currTimeSheetReportDetailModel.startDate.getTime() !==
      e.previousValue.getTime()
    ) {
      if (
        this.currTimeSheetReportDetailModel.endDate.getHours() <
        this.currTimeSheetReportDetailModel.startDate.getHours()
      ) {
        this.currTimeSheetReportDetailModel.endDate.setDate(
          this.currTimeSheetReportDetailModel.startDate.getDate() + 1
        );
      }
      if (
        this.currTimeSheetReportDetailModel.endDate.getHours() >
        this.currTimeSheetReportDetailModel.startDate.getHours()
      ) {
        var temp = new Date(
          this.chosenDate.getFullYear(),
          this.chosenDate.getMonth(),
          this.chosenDate.getDate(),
          this.currTimeSheetReportDetailModel.endDate.getHours(),
          this.currTimeSheetReportDetailModel.endDate.getMinutes(),
          0
        );
        this.currTimeSheetReportDetailModel.endDate = temp;
      }
      if (
        this.currTimeSheetReportDetailModel.endDate.getHours() ==
        this.currTimeSheetReportDetailModel.startDate.getHours()
      ) {
        if (
          this.currTimeSheetReportDetailModel.endDate.getMinutes() ==
          this.currTimeSheetReportDetailModel.startDate.getMinutes()
        ) {
          var temp = new Date(
            this.currTimeSheetReportDetailModel.startDate.getFullYear(),
            this.currTimeSheetReportDetailModel.startDate.getMonth(),
            this.currTimeSheetReportDetailModel.startDate.getDate(),
            this.currTimeSheetReportDetailModel.startDate.getHours(),
            this.currTimeSheetReportDetailModel.startDate.getMinutes(),
            this.currTimeSheetReportDetailModel.startDate.getMilliseconds()
          );
          this.currTimeSheetReportDetailModel.endDate = temp;
        }
        if (
          this.currTimeSheetReportDetailModel.endDate.getMinutes() >
          this.currTimeSheetReportDetailModel.startDate.getMinutes()
        ) {
          var temp = new Date(
            this.currTimeSheetReportDetailModel.startDate.getFullYear(),
            this.currTimeSheetReportDetailModel.startDate.getMonth(),
            this.currTimeSheetReportDetailModel.startDate.getDate(),
            this.currTimeSheetReportDetailModel.startDate.getHours(),
            this.currTimeSheetReportDetailModel.endDate.getMinutes(),
            this.currTimeSheetReportDetailModel.startDate.getMilliseconds()
          );
          this.currTimeSheetReportDetailModel.endDate = temp;
        }
        if (
          this.currTimeSheetReportDetailModel.endDate.getMinutes() <
          this.currTimeSheetReportDetailModel.startDate.getMinutes()
        ) {
          var temp = new Date(
            this.currTimeSheetReportDetailModel.startDate.getFullYear(),
            this.currTimeSheetReportDetailModel.startDate.getMonth(),
            this.currTimeSheetReportDetailModel.startDate.getDate() + 1,
            this.currTimeSheetReportDetailModel.startDate.getHours(),
            this.currTimeSheetReportDetailModel.endDate.getMinutes(),
            this.currTimeSheetReportDetailModel.startDate.getMilliseconds()
          );
          this.currTimeSheetReportDetailModel.endDate = temp;
        }
      }
    }
  }
  onValueChangedEndTime(e: any) {
    if (e.value === null) {
      this.currTimeSheetReportDetailModel.endDate = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
    if (this.currTimeSheetReportDetailModel.endDate !== e.previousValue) {
      if (
        this.currTimeSheetReportDetailModel.endDate.getHours() <
        this.currTimeSheetReportDetailModel.startDate.getHours()
      ) {
        this.currTimeSheetReportDetailModel.endDate.setDate(
          this.currTimeSheetReportDetailModel.startDate.getDate() + 1
        );
      }
      if (
        this.currTimeSheetReportDetailModel.endDate.getHours() >
        this.currTimeSheetReportDetailModel.startDate.getHours()
      ) {
        var temp = new Date(
          this.chosenDate.getFullYear(),
          this.chosenDate.getMonth(),
          this.chosenDate.getDate(),
          this.currTimeSheetReportDetailModel.endDate.getHours(),
          this.currTimeSheetReportDetailModel.endDate.getMinutes(),
          0
        );
        this.currTimeSheetReportDetailModel.endDate = temp;
      }
      if (
        this.currTimeSheetReportDetailModel.endDate.getHours() ==
        this.currTimeSheetReportDetailModel.startDate.getHours()
      ) {
        if (
          this.currTimeSheetReportDetailModel.endDate.getMinutes() ==
          this.currTimeSheetReportDetailModel.startDate.getMinutes()
        ) {
          var temp = new Date(
            this.currTimeSheetReportDetailModel.startDate.getFullYear(),
            this.currTimeSheetReportDetailModel.startDate.getMonth(),
            this.currTimeSheetReportDetailModel.startDate.getDate(),
            this.currTimeSheetReportDetailModel.startDate.getHours(),
            this.currTimeSheetReportDetailModel.startDate.getMinutes(),
            this.currTimeSheetReportDetailModel.startDate.getMilliseconds()
          );
          this.currTimeSheetReportDetailModel.endDate = temp;
        }
        if (
          this.currTimeSheetReportDetailModel.endDate.getMinutes() >
          this.currTimeSheetReportDetailModel.startDate.getMinutes()
        ) {
          var temp = new Date(
            this.currTimeSheetReportDetailModel.startDate.getFullYear(),
            this.currTimeSheetReportDetailModel.startDate.getMonth(),
            this.currTimeSheetReportDetailModel.startDate.getDate(),
            this.currTimeSheetReportDetailModel.startDate.getHours(),
            this.currTimeSheetReportDetailModel.endDate.getMinutes(),
            this.currTimeSheetReportDetailModel.startDate.getMilliseconds()
          );
          this.currTimeSheetReportDetailModel.endDate = temp;
        }
        if (
          this.currTimeSheetReportDetailModel.endDate.getMinutes() <
          this.currTimeSheetReportDetailModel.startDate.getMinutes()
        ) {
          var temp = new Date(
            this.currTimeSheetReportDetailModel.startDate.getFullYear(),
            this.currTimeSheetReportDetailModel.startDate.getMonth(),
            this.currTimeSheetReportDetailModel.startDate.getDate() + 1,
            this.currTimeSheetReportDetailModel.startDate.getHours(),
            this.currTimeSheetReportDetailModel.endDate.getMinutes(),
            this.currTimeSheetReportDetailModel.startDate.getMilliseconds()
          );
          this.currTimeSheetReportDetailModel.endDate = temp;
        }
      }
    }
  }
  onValueChangedDateRecord(e: any) {
    if (e.value === null) {
      this.chosenDate = new Date();
    } else {
      this.chosenDate = new Date();
      this.chosenDate = e.value;
    }

    this.currTimeSheetReportDetailModel.startDate.setFullYear(
      this.chosenDate.getFullYear()
    );
    this.currTimeSheetReportDetailModel.startDate.setMonth(
      this.chosenDate.getMonth()
    );
    this.currTimeSheetReportDetailModel.startDate.setDate(
      this.chosenDate.getDate()
    );

    this.currTimeSheetReportDetailModel.endDate.setFullYear(
      this.chosenDate.getFullYear()
    );
    this.currTimeSheetReportDetailModel.endDate.setMonth(
      this.chosenDate.getMonth()
    );
    this.currTimeSheetReportDetailModel.endDate.setDate(
      this.chosenDate.getDate()
    );

    let minuteEndTime = this.currTimeSheetReportDetailModel.endDate.getTime();
    let minuteStartTime =
      this.currTimeSheetReportDetailModel.startDate.getTime();
    if (minuteEndTime < minuteStartTime) {
      this.currTimeSheetReportDetailModel.endDate.setDate(
        this.currTimeSheetReportDetailModel.endDate.getDate() + 1
      );
    }

    this.dateboxStartTimeComp.option(
      'value',
      this.currTimeSheetReportDetailModel.startDate
    );
    this.dateboxEndTimeComp.option(
      'value',
      this.currTimeSheetReportDetailModel.endDate
    );

    this.dateboxEndTimeComp._renderValue();
    this.dateboxStartTimeComp._renderValue();
  }

  createButtonOptions = {
    icon: 'save',
    text: 'Save',
    onClick: (e) => {
      const valid = this.myForm.instance.validate();
      if (!valid.isValid) {
        return;
      }
      var item = new TimeSheetRecordModel();
      item.name = this.currTimeSheetReportDetailModel.timeSheetRecordName;
      item.startTime = this.commonService.convertSameDateTimeToDateUtc(
        this.currTimeSheetReportDetailModel.startDate
      );
      item.endTime = this.commonService.convertSameDateTimeToDateUtc(
        this.currTimeSheetReportDetailModel.endDate
      );
      item.tsActivityId = this.currTimeSheetReportDetailModel.tsActivityId;
      item.userId = this.currTimeSheetReportDetailModel.userId;
      item.backlogId = this.currTimeSheetReportDetailModel.backlogId;
      item.taskId = this.currTimeSheetReportDetailModel.taskId;
      this.timeSheetRecordService.add(item).subscribe(
        (response) => {
          this.commonService.UI.multipleNotify(
            'Time record has been created.',
            'success',
            2000
          );
          this.popupVisible = false;
          this.onRefreshGrid.emit();
        },
        (err) => {
          if (err.error === 'INVALID_MODEL_NAME_NULL') {
            this.commonService.UI.multipleNotify(
              'Name Task is required',
              'error',
              4000
            );
          }
          if (err.error === 'INVALID_MODEL_TASK_NAME_MAX_LENGTH') {
            this.commonService.UI.multipleNotify(
              'Name Task must be less than 200 characters',
              'error',
              4000
            );
          }
          if (err.error === 'INVALID_MODEL_BACKLOG_ID_MAX_LENGTH') {
            this.commonService.UI.multipleNotify(
              'Backlog Id must be less than 50 characters',
              'error',
              4000
            );
          }
          if (err.error === 'INVALID_MODEL_TASK_ID_MAX_LENGTH') {
            this.commonService.UI.multipleNotify(
              'Task Id must be less than 50 characters',
              'error',
              4000
            );
          }
          if (
            err.error ===
            'Task StartTime is greater than EndTime. Please check again'
          ) {
            this.commonService.UI.multipleNotify(
              'Task StartTime is greater than EndTime. Please check again',
              'error',
              4000
            );
          }
          this.onRefreshGrid.emit();
        }
      );
    },
  };

  editButtonOptions = {
    icon: 'save',
    text: 'Update',
    onClick: (e) => {
      const valid = this.myForm.instance.validate();
      if (!valid.isValid) {
        return;
      }
      var item = new TimeSheetRecordModel();
      item.id = this.model.data.timeSheetRecordId;
      item.name = this.currTimeSheetReportDetailModel.timeSheetRecordName;
      item.startTime = this.commonService.convertSameDateTimeToDateUtc(
        this.currTimeSheetReportDetailModel.startDate
      );
      item.endTime = this.commonService.convertSameDateTimeToDateUtc(
        this.currTimeSheetReportDetailModel.endDate
      );
      item.tsActivityId = this.currTimeSheetReportDetailModel.tsActivityId;
      item.userId = this.currTimeSheetReportDetailModel.userId;
      item.backlogId = this.currTimeSheetReportDetailModel.backlogId;
      item.taskId = this.currTimeSheetReportDetailModel.taskId;
      this.timeSheetRecordService.edit(item).subscribe(
        (response) => {
          this.commonService.UI.multipleNotify(
            'Successfully updated time record.',
            'success',
            2000
          );
          this.popupVisible = false;
          this.onRefreshGrid.emit();
        },
        (err) => {
          if (err.error === 'INVALID_MODEL_NAME_NULL') {
            this.commonService.UI.multipleNotify(
              'Name Task is required',
              'error',
              4000
            );
          }
          if (err.error === 'INVALID_RECORD_NOT_FOUND') {
            this.commonService.UI.multipleNotify(
              'Record not found',
              'error',
              4000
            );
          }
          if (err.error === 'INVALID_MODEL_TASK_NAME_MAX_LENGTH') {
            this.commonService.UI.multipleNotify(
              'Name Task must be less than 50 characters',
              'error',
              4000
            );
          }
          if (err.error === 'INVALID_MODEL_BACKLOG_ID_MAX_LENGTH') {
            this.commonService.UI.multipleNotify(
              'Backlog Id must be less than 20 characters',
              'error',
              4000
            );
          }
          if (err.error === 'INVALID_MODEL_TASK_ID_MAX_LENGTH') {
            this.commonService.UI.multipleNotify(
              'Task Id must be less than 20 characters',
              'error',
              4000
            );
          }
          if (err.error === 'TIMESHEET_LOCKED') {
            this.commonService.UI.multipleNotify(
              'Cannot edit record because Timesheet is locked',
              'error',
              4000
            );
          }
          if (
            err.error ===
            'Task StartTime is greater than EndTime. Please check again'
          ) {
            this.commonService.UI.multipleNotify(
              'Task StartTime is greater than EndTime. Please check again',
              'error',
              4000
            );
          }
          this.onRefreshGrid.emit();
        }
      );
    },
  };
}

@NgModule({
  imports: [
    BrowserModule,
    DxButtonModule,
    DxPopupModule,
    DxFormModule,
    DxValidatorModule,
    DxScrollViewModule,
    DxSelectBoxModule,
    DxValidationSummaryModule,
    DxTextBoxModule,
    DxDateBoxModule,
  ],
  declarations: [TimeSheetReportFormComponent],
  exports: [TimeSheetReportFormComponent],
})
export class TimeSheetReportFormModule {}

export class TimeSheetReportFormModel {
  state: TimeSheetReportFormState;
  data: TimeSheetReportDetailModel;

  constructor(init?: Partial<TimeSheetReportFormModel>) {
    Object.assign(this, init);
  }
}

export enum TimeSheetReportFormState {
  CREATE,
  EDIT,
}
