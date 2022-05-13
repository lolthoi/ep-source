import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { DxFormComponent } from 'devextreme-angular';
import ArrayStore from 'devextreme/data/array_store';
import DataSource from 'devextreme/data/data_source';
import {
  ActivityRecordViewModel,
  TimeSheetRecordModel,
} from 'src/app/shared/models/time-sheet-record.model';
import { WorkSpaceSettingRecordModel } from 'src/app/shared/models/work-space-setting.model';
import { AuthService } from 'src/app/shared/services';
import { CommonService } from 'src/app/shared/services/common.service';
import { TimeSheetRecordService } from 'src/app/shared/services/time-sheet-record.service';

@Component({
  selector: 'time-sheet-entry',
  templateUrl: './time-sheet-entry.component.html',
  styleUrls: ['./time-sheet-entry.component.scss'],
})
export class TimeSheetEntryComponent implements OnInit {
  @Input() entry: TimeSheetRecordModel;
  @Output() onRefreshGrid = new EventEmitter();
  @ViewChild(DxFormComponent, { static: false }) myform: DxFormComponent;

  isLoaded: boolean = false;
  popupConfirmDeleteVisible = false;
  isDisabledEntry: boolean;
  taskIdDataSource: any;
  entryDate = new Date();
  minRecordDate: Date;
  entryTotalTime: string;
  workSpaceSetting: WorkSpaceSettingRecordModel;
  initalEntry: TimeSheetRecordModel;
  selectBoxTaskComp: any;
  now = new Date();
  invalidTimeMessage: string = 'Wrong time format';
  invalidDateMessage: string = 'Wrong date format';

  constructor(
    private tsRecordService: TimeSheetRecordService,
    private commonService: CommonService,
    private authService: AuthService
  ) {
    this.initTaskIdDataSource();
  }

  ngOnInit(): void {
    this.initWorkSpaceSetting();
    this.calculateTotalTime(this.entry);
    this.entry.userId = this.authService.getUserValue.id;
    this.entryDate = new Date(this.entry.startTime);
    this.entry.startTime = this.tsRecordService.parseStringTimeZoneToDate(
      this.entry.startTime
    );
    this.entry.endTime = this.tsRecordService.parseStringTimeZoneToDate(
      this.entry.endTime
    );
    this.initalEntry = Object.assign({}, this.entry);
  }
  //Init data
  async initTaskIdDataSource() {
    await this.tsRecordService
      .getAllTaskId()
      .subscribe((res: ActivityRecordViewModel[]) => {
        var result = res.filter((x) => x.deletedBy === null);
        res.forEach((e) => {
          if (e.id === this.entry.tsActivityId) {
            var existed = result.filter((i) => i.id === e.id);
            if (existed !== null && existed.length === 0) {
              result.push(e);
            }
          }
        });

        this.taskIdDataSource = new DataSource({
          store: new ArrayStore({
            data: result,
            key: 'id',
          }),
          group: 'groupName',
        });
      });
  }
  onInitSelectTaskComp(e: any) {
    this.selectBoxTaskComp = e.component;
  }
  private initWorkSpaceSetting() {
    this.workSpaceSetting = new WorkSpaceSettingRecordModel();

    this.tsRecordService.getWsSetting().subscribe(
      (res: any) => {
        let wsData = new WorkSpaceSettingRecordModel();
        wsData.isLockTimeSheet = Boolean(JSON.parse(res.isLockTimeSheet));
        wsData.lockAfter = res.lockAfter;
        wsData.lockValueByDate = Number(JSON.parse(res.lockValueByDate));
        this.workSpaceSetting = wsData;
        //Set mindate record
        if (this.workSpaceSetting.isLockTimeSheet) {
          this.minRecordDate = new Date();
          let now = new Date();
          this.minRecordDate.setDate(
            now.getDate() - this.workSpaceSetting.lockValueByDate + 1
          );
        } else this.minRecordDate = new Date(1900, 1, 1);
        this.isLockedTimeSheet();
      },
      (err) => {}
    );
  }
  private isLockedTimeSheet() {
    if (this.workSpaceSetting.isLockTimeSheet) {
      let now = new Date();
      let lockedDateTime = new Date();
      var lockAfterArr = this.workSpaceSetting.lockAfter.split(':');

      lockedDateTime.setDate(
        now.getDate() - this.workSpaceSetting.lockValueByDate
      );
      lockedDateTime.setHours(parseInt(lockAfterArr[0]));
      lockedDateTime.setMinutes(parseInt(lockAfterArr[1]));
      lockedDateTime.setSeconds(parseInt(lockAfterArr[2]));
      lockedDateTime.setMilliseconds(0);
      if (
        this.tsRecordService
          .parseStringTimeZoneToDate(this.entry.startTime)
          .getTime() >= lockedDateTime.getTime()
      ) {
        this.isDisabledEntry = false;
      } else {
        this.isDisabledEntry = true;
        this.minRecordDate = new Date(1900, 1, 1);
      }
    } else this.isDisabledEntry = false;
    this.isLoaded = true;
  }

  //Valid data and edit
  private async validEntryFormAndEdit() {
    var instance = this.myform.instance.validate();
    if (!instance.isValid) {
      return;
    }
    let editedEntries = Object.assign({}, this.entry);
    editedEntries.startTime = this.commonService.convertSameDateTimeToDateUtc(
      this.entry.startTime
    );
    editedEntries.endTime = this.commonService.convertSameDateTimeToDateUtc(
      this.entry.endTime
    );
    var response = await this.tsRecordService.editReturnResponse(editedEntries);
    return response;
  }

  async executeUpdateChange() {
    let response = await this.validEntryFormAndEdit();
    if (response == undefined || response.error != null) {
      if (response.error === 'INVALID_RECORD_NOT_FOUND') {
        this.commonService.UI.multipleNotify('Record not found', 'error', 4000);
      }
      if (response.error === 'TIMESHEET_LOCKED') {
        this.commonService.UI.multipleNotify(
          'Timesheet is locked.',
          'error',
          4000
        );
      }
      if (response.error === 'INVALID_MODEL_NAME_NULL') {
        this.commonService.UI.multipleNotify(
          'Name Task is required',
          'error',
          4000
        );
      }
      if (response.error === 'INVALID_MODEL_TASK_NAME_MAX_LENGTH') {
        this.commonService.UI.multipleNotify(
          'Name Task must be less than 200 characters',
          'error',
          4000
        );
      }
      if (response.error === 'INVALID_MODEL_BACKLOG_ID_MAX_LENGTH') {
        this.commonService.UI.multipleNotify(
          'Backlog Id must be less than 50 characters',
          'error',
          4000
        );
      }
      if (response.error === 'INVALID_MODEL_TASK_ID_MAX_LENGTH') {
        this.commonService.UI.multipleNotify(
          'Task Id must be less than 50 characters',
          'error',
          4000
        );
      }
      if (
        response.error ===
        'Task StartTime is greater than EndTime. Please check again'
      ) {
        this.commonService.UI.multipleNotify(
          'Task StartTime is greater than EndTime. Please check again',
          'error',
          4000
        );
      }
    } else {
      this.commonService.UI.multipleNotify(
        'Successfully updated time entry.',
        'success',
        4000
      );
      this.onRefreshGrid.emit();
    }
  }

  onDeleteRecord(e) {
    this.popupConfirmDeleteVisible = true;
  }

  async onFieldChangeEvent(e, fieldString) {
    if (this.entry[fieldString] == this.initalEntry[fieldString]) return;
    this.executeUpdateChange();
  }

  //Change hour of startTime, endTime
  onChangeStartTime(e: any) {
    if (e.value === null) {
      this.entry.startTime = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }
    if (this.entry.startTime.getTime() !== e.previousValue.getTime()) {
      if (this.entry.endTime.getHours() < this.entry.startTime.getHours()) {
        this.entry.endTime.setDate(this.entry.endTime.getDate() + 1);
      }
      if (this.entry.endTime.getHours() > this.entry.startTime.getHours()) {
        var temp = new Date(
          this.entry.startTime.getFullYear(),
          this.entry.startTime.getMonth(),
          this.entry.startTime.getDate(),
          this.entry.endTime.getHours(),
          this.entry.endTime.getMinutes(),
          0
        );
        this.entry.endTime = temp;
      }
      if (this.entry.endTime.getHours() == this.entry.startTime.getHours()) {
        if (
          this.entry.endTime.getMinutes() == this.entry.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.entry.startTime.getFullYear(),
            this.entry.startTime.getMonth(),
            this.entry.startTime.getDate(),
            this.entry.startTime.getHours(),
            this.entry.startTime.getMinutes(),
            this.entry.startTime.getMilliseconds()
          );
          this.entry.endTime = temp;
        }
        if (
          this.entry.endTime.getMinutes() > this.entry.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.entry.startTime.getFullYear(),
            this.entry.startTime.getMonth(),
            this.entry.startTime.getDate(),
            this.entry.endTime.getHours(),
            this.entry.endTime.getMinutes(),
            this.entry.startTime.getMilliseconds()
          );
          this.entry.endTime = temp;
        }
        if (
          this.entry.endTime.getMinutes() < this.entry.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.entry.startTime.getFullYear(),
            this.entry.startTime.getMonth(),
            this.entry.startTime.getDate() + 1,
            this.entry.endTime.getHours(),
            this.entry.endTime.getMinutes(),
            this.entry.startTime.getMilliseconds()
          );
          this.entry.endTime = temp;
        }
      }
      this.executeUpdateChange();
    }
  }
  onChangeEndTime(e: any) {
    if (e.value === null) {
      this.entry.endTime = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        e.previousValue.getSeconds(),
        e.previousValue.getMilliseconds()
      );
    }

    if (this.entry.endTime.getTime() !== e.previousValue.getTime()) {
      if (this.entry.endTime.getHours() < this.entry.startTime.getHours()) {
        this.entry.endTime.setDate(this.entry.startTime.getDate() + 1);
      }
      if (this.entry.endTime.getHours() > this.entry.startTime.getHours()) {
        var temp = new Date(
          this.entry.startTime.getFullYear(),
          this.entry.startTime.getMonth(),
          this.entry.startTime.getDate(),
          this.entry.endTime.getHours(),
          this.entry.endTime.getMinutes(),
          0
        );

        this.entry.endTime = temp;
      }
      if (this.entry.endTime.getHours() == this.entry.startTime.getHours()) {
        if (
          this.entry.endTime.getMinutes() == this.entry.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.entry.startTime.getFullYear(),
            this.entry.startTime.getMonth(),
            this.entry.startTime.getDate(),
            this.entry.startTime.getHours(),
            this.entry.startTime.getMinutes(),
            this.entry.startTime.getMilliseconds()
          );
          this.entry.endTime = temp;
        }
        if (
          this.entry.endTime.getMinutes() > this.entry.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.entry.startTime.getFullYear(),
            this.entry.startTime.getMonth(),
            this.entry.startTime.getDate(),
            this.entry.endTime.getHours(),
            this.entry.endTime.getMinutes(),
            this.entry.startTime.getMilliseconds()
          );
          this.entry.endTime = temp;
        }
        if (
          this.entry.endTime.getMinutes() < this.entry.startTime.getMinutes()
        ) {
          var temp = new Date(
            this.entry.startTime.getFullYear(),
            this.entry.startTime.getMonth(),
            this.entry.startTime.getDate() + 1,
            this.entry.endTime.getHours(),
            this.entry.endTime.getMinutes(),
            this.entry.startTime.getMilliseconds()
          );
          this.entry.endTime = temp;
        }
      }
      this.executeUpdateChange();
    }
  }
  //Change date,year,month of starttime,endtime
  onValueChangedDateRecord(e: any) {
    if (e.value === null) {
      this.entryDate = new Date(
        e.previousValue.getFullYear(),
        e.previousValue.getMonth(),
        e.previousValue.getDate(),
        e.previousValue.getHours(),
        e.previousValue.getMinutes(),
        0
      );
    } else {
      this.entryDate = new Date();
      this.entryDate = e.value;
    }

    this.entry.startTime.setFullYear(this.entryDate.getFullYear());
    this.entry.startTime.setMonth(this.entryDate.getMonth());
    this.entry.startTime.setDate(this.entryDate.getDate());

    this.entry.endTime.setFullYear(this.entryDate.getFullYear());
    this.entry.endTime.setMonth(this.entryDate.getMonth());
    this.entry.endTime.setDate(this.entryDate.getDate());

    let minuteEndTime =
      this.entry.endTime.getHours() * 60 + this.entry.endTime.getMinutes();
    let minuteStartTime =
      this.entry.startTime.getHours() * 60 + this.entry.startTime.getMinutes();
    if (minuteEndTime < minuteStartTime) {
      this.entry.endTime.setDate(this.entry.endTime.getDate() + 1);
    }
    if (this.entryDate.getTime() !== e.previousValue.getTime()) {
      this.executeUpdateChange();
    }
  }
  //Calculate change time hour
  private calculateTotalTime(entry) {
    let diffMillisecond = this.tsRecordService.calculateDiffMs(
      this.entry.startTime,
      this.entry.endTime
    );
    this.entryTotalTime = this.tsRecordService.msToTime(diffMillisecond);
  }

  //Close button
  closeDeletePopupButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupConfirmDeleteVisible = false;
    },
  };

  confirmDeleteButtonOptions = {
    icon: 'save',
    text: 'Ok',
    onClick: (e) => {
      this.popupConfirmDeleteVisible = false;
      this.tsRecordService.delete(this.entry.id).subscribe(
        (next) => {
          this.commonService.UI.multipleNotify(
            'Time entry has been deleted.',
            'Success',
            2000
          );
          this.onRefreshGrid.emit();
        },
        (e: any) => {
          if (e.error === 'INVALID_RECORD_NOT_FOUND') {
            this.commonService.UI.multipleNotify(
              'Time entry not found',
              'error',
              4000
            );
          }
          if (e.error === 'No role.') {
            this.commonService.UI.multipleNotify(
              'No permission.',
              'error',
              4000
            );
          }
          if (e.error === 'TIMESHEET_LOCKED') {
            this.commonService.UI.multipleNotify(
              'Timesheet is locked.',
              'error',
              4000
            );
          }
        }
      );
    },
  };
}
