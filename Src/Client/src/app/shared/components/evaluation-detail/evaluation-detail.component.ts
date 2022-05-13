import {
  EvaluationUserDetailComponent,
  EvaluationUserDetailModule,
} from './../evaluation-user-detail/evaluation-user-detail.component';
import { Component, OnInit, NgModule, Input, ViewChild } from '@angular/core';
import { CommonService } from './../../services/common.service';
import { EvaluationLeaderDetailModule } from './../evaluation-leader-detail/evaluation-leader-detail.component';
import { EvaluationDetailService } from './evaluation-detail.service';
import {
  DxDataGridModule,
  DxButtonModule,
  DxTextBoxModule,
  DxCheckBoxModule,
  DxPopupModule,
  DxTextAreaModule,
  DxSelectBoxModule,
  DxValidatorModule,
  DxValidationSummaryModule,
  DxScrollViewModule,
} from 'devextreme-angular';

@Component({
  selector: 'app-evaluation-detail',
  templateUrl: './evaluation-detail.component.html',
  styleUrls: ['./evaluation-detail.component.scss'],
})
export class EvaluationDetailComponent implements OnInit {
  @ViewChild(EvaluationUserDetailComponent)
  userEvaluationChildComponent: EvaluationUserDetailComponent;
  @Input() id: any;
  @Input() quarterId: any;
  @Input() year: any;
  @Input() rootComp: any;
  @Input() name: any;
  @Input() projectName: any;
  isEdited = false;
  popupComp: any;
  isNewEvaluation = false;
  isdisabledSaveBtn = false;
  isLeader = false;
  visibleEdit: any;
  title = '';
  component = {
    // curent fn
    onShowPopUp: (
      role: boolean,
      point: any,
      quarter: any,
      year: any,
      createdDate: any,
      name: any,
      projectName: any
    ) => {
      this.title = 'Evaluation ' + name + ' of Project ' + projectName;
      this.component.onReloadResource();
      this.userEvaluationChildComponent.onInitData();
      if (role) {
        this.isLeader = true;
      }
      this.visibleEdit =
        this.allowEdit(point, quarter, year, createdDate) && this.isLeader;
      this.popupComp.show();

      if (this.isNewEvaluation && this.isLeader) {
        this.isEdited = true;
        this.component.onClickEditEvaluation();
      }
    },

    // parent fn

    // children fn
    getDataSource: null,
    onClickEditEvaluation: null,
    onClickCloseEvaluation: null,
    onReloadResource: null,
  };

  closeButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    hint: 'Cancel',
    onClick: (e: any) => {
      this.isEdited = false;
      this.isNewEvaluation = false;
      this.popupComp.hide();
      e.validationGroup.reset();
    },
  };
  saveButtonOptions = {
    text: 'Save',
    icon: 'save',
    hint: 'Save',
    onClick: (e: any) => {
      this.isdisabledSaveBtn = true;
      this.onSave(e);
    },
  };
  editButtonOptions = {
    text: 'Edit',
    hint: 'Edit',
    type: 'default',
    icon: 'edit',
    onClick: (e: any) => {
      this.onClickEditButton();
    },
  };
  constructor(
    private service: EvaluationDetailService,
    private common: CommonService
  ) {}
  onInitPopup = (e: any) => {
    this.popupComp = e.component;
  };
  onHiddenPopup = (e: any) => {
    this.component.onClickCloseEvaluation();
    this.isEdited = false;
    this.isNewEvaluation = false;
  };
  allowEdit = (point: any, quarter: any, year: any, createdDate: any) => {
    if (!createdDate) {
      return false;
    }
    if (point === '0') {
      this.isNewEvaluation = true;
    }
    return true;
  };
  getCurentQuarter = () => {
    const now = new Date().getMonth();
    if (now >= 4 && now <= 6) {
      return 2;
    } else if (now >= 7 && now <= 9) {
      return 3;
    } else if (now >= 10 && now <= 12) {
      return 4;
    } else {
      return 1;
    }
  };
  onClickEditButton = () => {
    this.isEdited = true;
    this.component.onClickEditEvaluation();
  };
  onSave = (e: any) => {
    const valid = e.validationGroup.validate();
    if (valid && valid.brokenRules && valid.brokenRules.length > 0) {
      this.common.UI.multipleNotify('Please rate all items!!', 'warning', 2000);
      valid.brokenRules[0].validator.focus();
      this.isdisabledSaveBtn = false;
    }

    if (valid.isValid) {
      this.isEdited = false;
      const data = this.component.getDataSource();
      this.service.createQuarterEvaluation(this.quarterId, data).subscribe(
        (result: any) => {
          this.isdisabledSaveBtn = false;
          this.rootComp.onRefreshGrid();
          this.popupComp.hide();
          this.common.UI.multipleNotify(
            'You have just updated successfully evaluating your team member',
            'success',
            2000
          );
        },
        (err: any) => {
          let errorMess = 'Error! An error occurred. Please try again later.';
          switch (err.error) {
            case 'NOT_THE_PROJECT_LEADER_OR_ADMIN':
              errorMess = 'You are not admin or leader of this project';
              break;
            case 'NOT_INTIME_EVALUATION':
              errorMess = 'Now is not the time to evaluation';
              break;
            default:
              break;
          }

          this.isdisabledSaveBtn = false;
          this.common.UI.multipleNotify(errorMess, 'error', 2000);
          this.popupComp.hide();
        }
      );
    }
  };
  ngOnInit(): void {
    Object.assign(this.rootComp, this.component);
  }
}

@NgModule({
  imports: [
    DxDataGridModule,
    DxButtonModule,
    DxTextBoxModule,
    DxCheckBoxModule,
    DxPopupModule,
    DxTextAreaModule,
    DxSelectBoxModule,
    DxValidatorModule,
    DxValidationSummaryModule,
    DxScrollViewModule,
    EvaluationLeaderDetailModule,
    EvaluationUserDetailModule,
  ],
  exports: [EvaluationDetailComponent],
  declarations: [EvaluationDetailComponent],
  bootstrap: [EvaluationDetailComponent],
})
export class EvaluationDetailModule {}
