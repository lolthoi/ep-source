import {
  ActivityGroupRecordViewModel,
  ActivityRecordViewModel,
  TimeSheetRecordModel,
} from './../../../models/time-sheet-record.model';
import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { TimeSheetRecordService } from 'src/app/shared/services/time-sheet-record.service';
import DataSource from 'devextreme/data/data_source';
import ArrayStore from 'devextreme/data/array_store';
import { WorkSpaceSettingService } from 'src/app/shared/services/work-space-setting.service';
import { WorkSpaceSettingRecordModel } from 'src/app/shared/models/work-space-setting.model';
import { DxFormComponent } from 'devextreme-angular';
import { CommonService } from 'src/app/shared/services/common.service';
import { AuthService } from 'src/app/shared/services';

@Component({
  selector: 'time-tracker-recorder',
  templateUrl: './time-tracker-recorder.component.html',
  styleUrls: ['./time-tracker-recorder.component.scss'],
})
export class TimeTrackerRecorderComponent implements OnInit {
  @Output() onRefreshGrid = new EventEmitter();

  workSpaceSetting: WorkSpaceSettingRecordModel;
  record: TimeSheetRecordModel;
  now = new Date();
  chosenDate = new Date();
  minRecordDate: Date;
  taskIdDataSource: any;
  totalTime: string;
  @ViewChild(DxFormComponent, { static: false }) myform: DxFormComponent;
  dateboxStartTimeComp: any;
  dateboxEndTimeComp: any;
  invalidTimeMessage: string = 'Wrong time format';
  invalidDateMessage: string = 'Wrong date format';

  constructor(
    private tsRecordService: TimeSheetRecordService,
    private workSpaceSettingService: WorkSpaceSettingService,
    private common: CommonService,
    private authService: AuthService
  ) {
    this.initTaskIdDataSource();
    this.initWorkSpaceSetting();
  }

  //#region Init data
  async initTaskIdDataSource() {
    await this.tsRecordService
      .getAllTaskId()
      .subscribe((res: ActivityRecordViewModel[]) => {
        var result = res.filter((x) => x.deletedBy === null);
        this.taskIdDataSource = new DataSource({
          store: new ArrayStore({
            data: result,
            key: 'id',
          }),
          group: 'groupName',
        });
      });
  }

  private initWorkSpaceSetting() {
    this.workSpaceSettingService.getWorkSpaceSettings().subscribe((next) => {
      this.workSpaceSetting = new WorkSpaceSettingRecordModel();
      this.workSpaceSetting.isLockTimeSheet = Boolean(
        JSON.parse(next.isLockTimeSheet)
      );
      this.workSpaceSetting.lockAfter = next.lockAfter;
      this.workSpaceSetting.lockValueByDate = Number(
        JSON.parse(next.lockValueByDate)
      );

      //Set mindate record
      if (this.workSpaceSetting.isLockTimeSheet) {
        this.minRecordDate = new Date();
        let now = new Date();
        this.minRecordDate.setDate(
          now.getDate() - this.workSpaceSetting.lockValueByDate + 1
        );
      } else this.minRecordDate = new Date(1900, 1, 1);
    });
  }
  ////#endregion Init data

  onInitStartTimeComp(e) {
    this.dateboxStartTimeComp = e.component;
  }

  onInitEndTimeComp(e) {
    this.dateboxEndTimeComp = e.component;
  }

  //Main function: CREATE TIMESHEET RECORD
  onSaveTimeSheetRecord(e: any) {
    var instance = this.myform.instance.validate();
    if (!instance.isValid) {
      return;
    }
    let tsRecordEntity = Object.assign({}, this.record);

    tsRecordEntity.startTime = this.common.convertSameDateTimeToDateUtc(
      this.record.startTime
    );
    tsRecordEntity.endTime = this.common.convertSameDateTimeToDateUtc(
      this.record.endTime
    );

    this.tsRecordService.add(tsRecordEntity).subscribe(
      (next) => {
        //SUCCESS ADD TIMESHEET-RECORD
        this.myform.instance.resetValues();
        this.initBlankRecordModel();
        this.onRefreshGrid.emit();
        this.common.UI.multipleNotify(
          'Time entry has been created.',
          'success',
          2000
        );
      },
      (error) => {
        //ERROR ADD TIMESHEET-RECORD
        if (error.error === 'TIMESHEET_LOCKED') {
          this.common.UI.multipleNotify('Timesheet is locked.', 'error', 4000);
        }
        if (error.error === 'INVALID_MODEL_NAME_NULL') {
          this.common.UI.multipleNotify('Name Task is required', 'error', 4000);
        }
        if (error.error === 'INVALID_MODEL_TASK_NAME_MAX_LENGTH') {
          this.common.UI.multipleNotify(
            'Name Task must be less than 200 characters',
            'error',
            4000
          );
        }
        if (error.error === 'INVALID_MODEL_BACKLOG_ID_MAX_LENGTH') {
          this.common.UI.multipleNotify(
            'Backlog Id must be less than 50 characters',
            'error',
            4000
          );
        }
        if (error.error === 'INVALID_MODEL_TASK_ID_MAX_LENGTH') {
          this.common.UI.multipleNotify(
            'Task Id must be less than 50 characters',
            'error',
            4000
          );
        }
        if (
          error.error ===
          'Task StartTime is greater than EndTime. Please check again'
        ) {
          this.common.UI.multipleNotify(
            'Task StartTime is greater than EndTime. Please check again',
            'error',
            4000
          );
        }
      }
    );
  }

  //#region Handle StartTime,EndTime
  onValueChangedStartTime(e: any) {
    if (this.record.startTime === null) {
      this.record.startTime = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
    if (this.record.startTime.getTime() !== e.previousValue.getTime()) {
      if (this.record.endTime.getHours() < this.record.startTime.getHours()) {
        this.record.endTime.setDate(this.record.startTime.getDate() + 1);
        this.calculateTotalTime(this.record);
        return;
      }
      if (this.record.endTime.getHours() > this.record.startTime.getHours()) {
        var temp = new Date(
          this.chosenDate.getFullYear(),
          this.chosenDate.getMonth(),
          this.chosenDate.getDate(),
          this.record.endTime.getHours(),
          this.record.endTime.getMinutes(),
          0
        );
        this.record.endTime = temp;
      }
      if (this.record.endTime.getHours() == this.record.startTime.getHours()) {
        if (
          this.record.endTime.getMinutes() == this.record.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.record.startTime.getFullYear(),
            this.record.startTime.getMonth(),
            this.record.startTime.getDate(),
            this.record.startTime.getHours(),
            this.record.startTime.getMinutes(),
            this.record.startTime.getMilliseconds()
          );
          this.record.endTime = temp;
        }
        if (
          this.record.endTime.getMinutes() > this.record.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.record.startTime.getFullYear(),
            this.record.startTime.getMonth(),
            this.record.startTime.getDate(),
            this.record.startTime.getHours(),
            this.record.endTime.getMinutes(),
            this.record.startTime.getMilliseconds()
          );
          this.record.endTime = temp;
        }
        if (
          this.record.endTime.getMinutes() < this.record.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.record.startTime.getFullYear(),
            this.record.startTime.getMonth(),
            this.record.startTime.getDate() + 1,
            this.record.startTime.getHours(),
            this.record.endTime.getMinutes(),
            this.record.startTime.getMilliseconds()
          );
          this.record.endTime = temp;
        }
      }
      this.calculateTotalTime(this.record);
    }
    console.log('after choose start time: startTime ' + this.record.startTime);
    console.log('after choose start time: endTime ' + this.record.endTime);
  }

  onValueChangedEndTime(e: any) {
    if (this.record.endTime === null) {
      this.record.endTime = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
    if (this.record.endTime !== e.previousValue) {
      if (this.record.endTime.getHours() < this.record.startTime.getHours()) {
        this.record.endTime.setDate(this.record.startTime.getDate() + 1);
        this.calculateTotalTime(this.record);
        return;
      }
      if (this.record.endTime.getHours() > this.record.startTime.getHours()) {
        var temp = new Date(
          this.chosenDate.getFullYear(),
          this.chosenDate.getMonth(),
          this.chosenDate.getDate(),
          this.record.endTime.getHours(),
          this.record.endTime.getMinutes(),
          0
        );
        this.record.endTime = temp;
      }
      if (this.record.endTime.getHours() == this.record.startTime.getHours()) {
        if (
          this.record.endTime.getMinutes() == this.record.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.record.startTime.getFullYear(),
            this.record.startTime.getMonth(),
            this.record.startTime.getDate(),
            this.record.startTime.getHours(),
            this.record.startTime.getMinutes(),
            this.record.startTime.getMilliseconds()
          );
          this.record.endTime = temp;
        }
        if (
          this.record.endTime.getMinutes() > this.record.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.record.startTime.getFullYear(),
            this.record.startTime.getMonth(),
            this.record.startTime.getDate(),
            this.record.startTime.getHours(),
            this.record.endTime.getMinutes(),
            this.record.startTime.getMilliseconds()
          );
          this.record.endTime = temp;
        }
        if (
          this.record.endTime.getMinutes() < this.record.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.record.startTime.getFullYear(),
            this.record.startTime.getMonth(),
            this.record.startTime.getDate() + 1,
            this.record.startTime.getHours(),
            this.record.endTime.getMinutes(),
            this.record.startTime.getMilliseconds()
          );
          this.record.endTime = temp;
        }
      }
      this.calculateTotalTime(this.record);
    }
    console.log('after choose end time: startTime ' + this.record.startTime);
    console.log('after choose end time: endTime ' + this.record.endTime);
  }

  //Change date,year,month of starttime,endtime
  onValueChangedDateRecord(e: any) {
    if (e.value === null) {
      this.chosenDate = new Date();
    } else {
      this.chosenDate = new Date();
      this.chosenDate = e.value;
    }

    this.record.startTime.setFullYear(this.chosenDate.getFullYear());
    this.record.startTime.setMonth(this.chosenDate.getMonth());
    this.record.startTime.setDate(this.chosenDate.getDate());

    this.record.endTime.setFullYear(this.chosenDate.getFullYear());
    this.record.endTime.setMonth(this.chosenDate.getMonth());
    this.record.endTime.setDate(this.chosenDate.getDate());

    let minuteEndTime = this.record.endTime.getTime();
    let minuteStartTime = this.record.startTime.getTime();
    if (minuteEndTime < minuteStartTime) {
      this.record.endTime.setDate(this.record.endTime.getDate() + 1);
    }

    this.dateboxStartTimeComp.option('value', this.record.startTime);
    this.dateboxEndTimeComp.option('value', this.record.endTime);

    this.dateboxEndTimeComp._renderValue();
    this.dateboxStartTimeComp._renderValue();

    console.log('after choose date: startTime ' + this.record.startTime);
    console.log('after choose date: endTime ' + this.record.endTime);

    this.calculateTotalTime(this.record);
  }

  private calculateTotalTime(entry) {
    let diffMillisecond = this.tsRecordService.calculateDiffMs(
      this.record.startTime,
      this.record.endTime
    );
    this.totalTime = this.tsRecordService.msToTime(diffMillisecond);
  }
  //#endregion

  onActivityIdValueChanged(event) {
    this.record.tsActivityId = event.value;
  }

  initBlankRecordModel() {
    this.record = new TimeSheetRecordModel();
    this.record.name = '';
    this.record.startTime = new Date();
    this.record.endTime = new Date();
    this.record.startTime.setSeconds(0);
    this.record.startTime.setMilliseconds(0);
    this.record.endTime.setSeconds(0);
    this.record.endTime.setMilliseconds(0);
    this.record.userId = this.authService.getUserValue.id;
    this.now = new Date();
    this.chosenDate = new Date();
  }

  ngOnInit(): void {
    this.initBlankRecordModel();
    this.now = new Date();
    this.now.setSeconds(0);
    this.now.setMilliseconds(0);
    this.chosenDate = new Date();
    this.chosenDate.setSeconds(0);
    this.chosenDate.setMilliseconds(0);
    this.calculateTotalTime(this.record);
  }
}
