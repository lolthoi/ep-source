import { Component, NgModule, OnInit, ViewChild } from '@angular/core';
import {
  DxCheckBoxModule,
  DxDateBoxModule,
  DxFormComponent,
  DxFormModule,
  DxNumberBoxModule,
  DxValidatorModule,
} from 'devextreme-angular';
import { WorkSpaceSettingService } from '../../shared/services/work-space-setting.service';
import {
  WorkSpaceSettingModel,
  WorkSpaceSettingForm,
} from '../../shared/models/work-space-setting.model';
import { CommonService } from '../../shared/services/common.service';
import { AuthService } from '../../shared/services';

@Component({
  selector: 'app-work-space-setting',
  templateUrl: './work-space-setting.component.html',
  styleUrls: ['./work-space-setting.component.scss'],
})
export class WorkSpaceSettingComponent implements OnInit {
  @ViewChild(DxFormComponent, { static: false }) myForm: DxFormComponent;
  workSpaceSettingModel: WorkSpaceSettingModel = null;
  loading = false;
  isDisable = false;
  isAdminRole = false;

  workSpaceSettingForm: WorkSpaceSettingForm = new WorkSpaceSettingForm();

  constructor(
    private authService: AuthService,
    private workSpaceSettingService: WorkSpaceSettingService,
    private commonService: CommonService
  ) {
    this.isAdminRole = this.authService.isRoleAdministrator;
  }
  ngOnInit() {
    this.onInit();
  }

  onInit() {
    this.workSpaceSettingService.getWorkSpaceSettings().subscribe(
      (response: WorkSpaceSettingModel) => {
        this.workSpaceSettingForm.lockTime = Boolean(
          JSON.parse(response.isLockTimeSheet)
        );
        var letTime = response.lockAfter.split(':');
        this.workSpaceSettingForm.lockingTime = new Date(
          new Date().getFullYear(),
          new Date().getMonth(),
          new Date().getDay(),
          Number(letTime[0]),
          Number(letTime[1]),
          Number(letTime[2])
        );
        this.workSpaceSettingForm.timesheetLockCycle = Number(
          JSON.parse(response.lockValueByDate)
        );
      },
      (error) => {
        this.commonService.UI.multipleNotify(error.error, 'error', 2000);
      }
    );
  }

  onValidationCallBackLockingTime = (e: any) => {
    if (this.workSpaceSettingForm.lockTime == true) {
      if (e === undefined) {
        return false;
      }
      if (
        this.workSpaceSettingForm.lockingTime === null ||
        this.workSpaceSettingForm.lockingTime === undefined
      ) {
        return false;
      }
      return true;
    } else {
      return true;
    }
  };

  onValidationCallBackTimesheetLockCycle = (e: any) => {
    if (this.workSpaceSettingForm.lockTime == true) {
      if (e === undefined) {
        return false;
      }
      if (
        this.workSpaceSettingForm.timesheetLockCycle === null ||
        this.workSpaceSettingForm.timesheetLockCycle === undefined
      ) {
        return false;
      }
      return true;
    } else {
      return true;
    }
  };

  onValidationCallBackMinTimesheetLockCycle = (e: any) => {
    if (this.workSpaceSettingForm.lockTime == true) {
      if (e === undefined) {
        return false;
      }
      if (this.workSpaceSettingForm.timesheetLockCycle < 1) {
        return false;
      }
      return true;
    } else {
      return true;
    }
  };

  onValidationCallBackMaxTimesheetLockCycle = (e: any) => {
    if (this.workSpaceSettingForm.lockTime == true) {
      if (e === undefined) {
        return false;
      }
      if (this.workSpaceSettingForm.timesheetLockCycle > 999999) {
        return false;
      }
      return true;
    } else {
      return true;
    }
  };

  onSubmit(e) {
    e.preventDefault();
    this.workSpaceSettingModel = new WorkSpaceSettingModel();
    this.workSpaceSettingModel.isLockTimeSheet = String(
      this.workSpaceSettingForm.lockTime
    );
    this.workSpaceSettingModel.lockValueByDate = String(
      this.workSpaceSettingForm.timesheetLockCycle
    );
    if (this.workSpaceSettingForm.lockTime === true) {
      if (
        this.workSpaceSettingForm.timesheetLockCycle === null ||
        this.workSpaceSettingForm.timesheetLockCycle === undefined
      ) {
        this.commonService.UI.multipleNotify(
          'Please enter the timesheet lock cycle.',
          'error',
          2000
        );
        return;
      }

      if (
        this.workSpaceSettingForm.lockingTime === null ||
        this.workSpaceSettingForm.lockingTime === undefined
      ) {
        this.commonService.UI.multipleNotify(
          'Please enter the timesheet lock cycle.',
          'error',
          2000
        );
        return;
      }
    } else {
      this.workSpaceSettingForm.lockingTime = new Date();
      this.workSpaceSettingForm.timesheetLockCycle = 1;
    }

    this.workSpaceSettingModel.lockAfter =
      new Date(this.workSpaceSettingForm.lockingTime).getHours() +
      ':' +
      new Date(this.workSpaceSettingForm.lockingTime).getMinutes() +
      ':00';

    this.workSpaceSettingService.edit(this.workSpaceSettingModel).subscribe(
      (response: WorkSpaceSettingModel) => {
        this.onInit();
        this.commonService.UI.multipleNotify(
          'Update work space setting success',
          'success',
          2000
        );
      },
      (error) => {
        this.commonService.UI.multipleNotify(error.error, 'error', 2000);
      }
    );
  }
}

@NgModule({
  imports: [
    DxFormModule,
    DxCheckBoxModule,
    DxDateBoxModule,
    DxNumberBoxModule,
    DxValidatorModule,
  ],
  declarations: [WorkSpaceSettingComponent],
  bootstrap: [WorkSpaceSettingComponent],
})
export class WorkSpaceSettingModule {}
