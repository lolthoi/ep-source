import { Component, NgModule, OnInit, ViewChild } from '@angular/core';
import {
  DxButtonModule,
  DxDataGridModule,
  DxLoadPanelModule,
  DxPopupModule,
  DxResponsiveBoxModule,
  DxSelectBoxModule,
  DxValidatorModule,
} from 'devextreme-angular';
import { BrowserModule } from '@angular/platform-browser';

import { EvaluationPersonalModel } from '../../../shared/models/evaluation-personal.model';
import { EvaluationPersonalService } from '../../../shared/services/evaluation-personal.service';

import { AuthService } from '../../../shared/services/auth.service';
import { ProjectService } from '../../../shared/services/project.service';
import { ProjectModel } from '../../../shared/models/project.model';
import { CommonService } from '../../../shared/services/common.service';
import {
  PopupEvaluatePersonalComponent,
  PopupEvaluatePersonalModule,
} from './popup-evalate-personal/popup-evalate-personal.component';
import {
  EvaluationDetailModule,
  EvaluationDetailComponent,
} from './../../../shared/components/evaluation-detail/evaluation-detail.component';
import { MailService } from 'src/app/shared/services/mail.service';
import { QuarterEvaluationIdModel } from 'src/app/shared/models/quarter-evaluation-id.model';
import { QuarterEvaluationModel } from 'src/app/shared/models/quarter-evaluation.model';

@Component({
  selector: 'app-evaluation-personal',
  templateUrl: './evaluation-personal.component.html',
  styleUrls: ['./evaluation-personal.component.scss'],
})
export class EvaluationPersonalComponent implements OnInit {
  //#region  Init variable
  @ViewChild(EvaluationDetailComponent)
  evaluationDetailComponent: EvaluationDetailComponent;

  dataSource: EvaluationPersonalModel[];
  gridColumns = [
    'quarterText',
    'position',
    'pointAverage',
    'leader',
    'project',
  ];
  loading = false;
  currentYears = new Date().getFullYear();
  years = [];
  listProjects: ProjectModel[] = [];
  root = {};
  formYearSelected = 0;
  toYearSelected = 0;
  projectSelected = 0;
  quarterEvaluations: QuarterEvaluationModel[];
  quarterEvaluationId: any;
  @ViewChild(PopupEvaluatePersonalComponent) popupComponent;

  userCurrent: string = '';
  validatorCustomFromYear: any;
  validatorCustomToYear: any;
  //#endregion

  constructor(
    private evaluationPersonalService: EvaluationPersonalService,
    private authService: AuthService,
    private commonService: CommonService,
    private projectService: ProjectService,
    private mailServive: MailService
  ) {
    this.userCurrent =
      authService.getUser.firstName + ' ' + authService.getUser.lastName;
  }
  eventClick(e) {
    
    if (e.data.id === this.commonService.getGuidEmpty()) {
      this.commonService.UI.multipleNotify(
        'No Evaluation for this quarter!!!',
        'error',
        2000
      );
      return;
    }
    this.quarterEvaluationId = e.data.id;
    setTimeout(() => {
      this.evaluationDetailComponent.component.onShowPopUp(
        false,
        e.data.pointAverage,
        e.data.quarter,
        e.data.year,
        '',
        this.userCurrent,
        e.data.project
      );
    }, 10);
  }

  ngOnInit(): void {
    this.loading = true;
    this.loadProjects();
    this.loadYears();
  }

  loadYears() {
    for (var i = this.currentYears; i > 2010; i--) {
      this.years.push(i);
    }
    this.formYearSelected = this.currentYears - 1;
    this.toYearSelected = this.currentYears;
    this.getEvaluationPersonals();
  }

  loadProjects() {
    this.projectService.GetAllProjectOfUser().subscribe(
      (responseData: ProjectModel[]) => {
        this.listProjects = [];
        if (responseData.length > 0) {
          this.listProjects = responseData;
        }
      },
      (error) => {
        this.commonService.UI.multipleNotify(error.error, 'error', 2000);
      }
    );
  }

  getEvaluationPersonals() {
    if (this.formYearSelected > this.toYearSelected) {
      //this.commonService.UI.multipleNotify("The date in the field from must be less than the date in field to", 'error', 2000);
      //this.dataSource = [];
      this.loading = false;
      return;
    }
    this.evaluationPersonalService
      .getEvaluationPersonals(
        this.formYearSelected,
        this.toYearSelected,
        this.projectSelected
      )
      .subscribe(
        (responseData: EvaluationPersonalModel[]) => {
          this.dataSource = [];
          if (responseData.length > 0) {
            this.dataSource = responseData;
          }
          this.loading = false;
        },
        (error) => {
          this.commonService.UI.multipleNotify(error.error, 'error', 2000);
          this.loading = false;
        }
      );
  }

  onInitializedCustomFromYear = (e) => {
    this.validatorCustomFromYear = e.component;
  };
  onInitializedCustomToYear = (e) => {
    this.validatorCustomToYear = e.component;
  };

  customFromYearRule = [
    {
      type: 'custom',
      reevaluate: true,
      validationCallback: function (e) {
        return this.toYearSelected >= this.formYearSelected;
      }.bind(this),
      message:
        'The date in the field from must be less than the date in field to',
    },
  ];

  customToYearRule = [
    {
      type: 'custom',
      reevaluate: true,
      validationCallback: function (e) {
        return this.toYearSelected >= this.formYearSelected;
      }.bind(this),
      message:
        'The date in the field from must be less than the date in field to',
    },
  ];

  onFormYearChanged(e) {
    this.validatorCustomFromYear.validate();
    if (this.validatorCustomFromYear.validate().isValid === true) {
      this.validatorCustomToYear.validate();
    }
    if (e.value === null) {
      this.formYearSelected = 0;
    } else {
      this.formYearSelected = e.value;
    }
    this.loading = true;
    this.getEvaluationPersonals();
  }

  onToYearChanged(e) {
    this.validatorCustomToYear.validate();
    if (this.validatorCustomToYear.validate().isValid === true) {
      this.validatorCustomFromYear.validate();
    }
    if (e.value === null) {
      this.toYearSelected = 0;
    } else {
      this.toYearSelected = e.value;
    }
    this.loading = true;

    this.getEvaluationPersonals();
  }
  onProjectChanged(e) {
    if (e.value === null) {
      this.projectSelected = 0;
    } else {
      this.projectSelected = e.value;
    }
    this.loading = true;
    this.getEvaluationPersonals();
  }

  onClickSelfEvaluateButton(e) {
    this.popupComponent.openPopUp();
  }

  screen(width) {
    return width < 700 ? 'sm' : 'lg';
  }
}
@NgModule({
  imports: [
    BrowserModule,
    DxDataGridModule,
    DxLoadPanelModule,
    DxResponsiveBoxModule,
    DxButtonModule,
    DxSelectBoxModule,
    PopupEvaluatePersonalModule,
    EvaluationDetailModule,
    DxValidatorModule,
  ],
  declarations: [EvaluationPersonalComponent],
  bootstrap: [EvaluationPersonalComponent],
})
export class EvaluationPersonalModule {}
