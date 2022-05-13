import { AuthService } from './../../services/auth.service';
import { UserEvaluationService } from './../../services/evaluation-service';
import {
  Component,
  Input,
  NgModule,
  OnInit,
  Output,
  EventEmitter,
  OnDestroy,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { UserEvaluationModel } from '../../models/evaluation-user.model';
import {
  DxBoxModule,
  DxButtonModule,
  DxFormComponent,
  DxFormModule,
  DxPopupModule,
  DxSelectBoxModule,
  DxTextAreaModule,
  DxTextBoxModule,
  DxValidatorModule,
} from 'devextreme-angular';
import { EvaluationPersonalService } from '../../services/evaluation-personal.service';
import { CommonService } from '../../services/common.service';
import {
  DataSourcePersonalEvaluateModel,
  ProjectDataSourceModel,
  QuarterDataSourceModel,
  YearDataSourceModel,
} from '../../models/datasource-personal-evaluate.model';

@Component({
  selector: 'app-evaluation-user-detail',
  templateUrl: './evaluation-user-detail.component.html',
  styleUrls: ['./evaluation-user-detail.component.scss'],
})
export class EvaluationUserDetailComponent implements OnInit {
  @Input() quarterEvaluationId: any;
  @Input() readOnly: boolean;
  @Input() isUserSelfEvaluated: boolean;
  @Input() isDoneSelectQuarter: boolean = false;
  @Input() popupVisible: boolean = false;
  @Output() onSuccessSavePersonalEvaluate = new EventEmitter<void>();
  @Output() onShowSubmitButton = new EventEmitter<boolean>();

  @ViewChild(DxFormComponent, { static: false }) myform: DxFormComponent;
  popupEditState: EnumPersonalEvaluateFormState =
    EnumPersonalEvaluateFormState.CREATE;
  currModel: UserEvaluationModel = new UserEvaluationModel();
  avaiableModel: DataSourcePersonalEvaluateModel =
    new DataSourcePersonalEvaluateModel();

  yearDatasource: YearDataSourceModel[] = [];
  quarterDataSource: QuarterDataSourceModel[] = [];
  projectDatasource: ProjectDataSourceModel[] = [];
  selectedYear: number = null;
  selectedQuarter: number = null;
  selectedProject: number = null;
  isHavingEvaluate: boolean = null;

  constructor(
    private userEvaluationService: UserEvaluationService,
    private evaluationPersonalService: EvaluationPersonalService,
    private common: CommonService,
    private authSerice: AuthService
  ) {}

  ngOnInit(): void {
    this.currModel.quarterEvaluationId = this.quarterEvaluationId;
  }

  OnDestroy(): void {}

  //#region Init data when user self evaluate
  onInitSelfEvaluate() {
    var currentId = this.authSerice.getUserValue.id.toString();
    this.evaluationPersonalService
      .getAvaiableQuarterEvaluations(currentId)
      .subscribe(
        (next) => {
          this.avaiableModel = next;
          this.isHavingEvaluate = next.isAvaibleEvaluate;
          this.yearDatasource = next.yearSource;
        },
        (error) => {}
      );
  }
  //#endregion

  //#region Init when defined it has quarterEvaluationId

  onInitData() {
    this.userEvaluationService.getById(this.quarterEvaluationId).subscribe(
      (next) => {
        this.currModel = next;
        this.popupEditState = EnumPersonalEvaluateFormState.EDIT;
        //If state is readonly => leader evaluate => show information about member self evaluate
        if (this.readOnly == true) {
          this.isUserSelfEvaluated = true;
        }
      },
      (error) => {
        if (error.status === 404) {
          this.popupEditState = EnumPersonalEvaluateFormState.CREATE;
          this.currModel = new UserEvaluationModel();
          //If state is readonly => leader evaluate => show user notevaluate yet in ngIf condition
          if (this.readOnly == true) {
            this.isUserSelfEvaluated = false;
          }
        }
      }
    );
  }
  //#endregion

  //#region FUNCTION BUTTON

  onCancelPopupPersonalEvaluation() {
    //reset
    this.currModel = new UserEvaluationModel();
    this.currModel.quarterEvaluationId = this.quarterEvaluationId;
    this.popupEditState = EnumPersonalEvaluateFormState.CREATE;

    this.selectedYear = null;
    this.selectedQuarter = null;
    this.selectedProject = null;
    this.isDoneSelectQuarter = false;
    this.onShowSubmitButton.emit(false);
  }

  onSubmitPersonalEvaluation() {
    if (this.myform.instance.validate().isValid) {
      this.currModel.quarterEvaluationId = this.quarterEvaluationId;
      switch (this.popupEditState) {
        case EnumPersonalEvaluateFormState.CREATE:
          this.evaluationPersonalService
            .createUserQuarterEvaluation(this.currModel)
            .subscribe(
              (next) => {
                this.onSuccessSavePersonalEvaluate.emit();
                this.common.UI.multipleNotify(
                  'EVALUATE SUCCESS',
                  'success',
                  2000
                );
              },
              (error) => {
                if (error.error == 'EVALIATION DATE IS EXPIRED') {
                  this.common.UI.multipleNotify(
                    'EVALIATION DATE IS EXPIRED',
                    'error',
                    2000
                  );
                } else {
                  this.common.UI.multipleNotify(
                    'QUARTER_EVALUATION_DUPLICATE',
                    'error',
                    2000
                  );
                }
              }
            );

          break;
        case EnumPersonalEvaluateFormState.EDIT:
          this.evaluationPersonalService
            .updateUserQuarterEvaluation(this.currModel)
            .subscribe(
              (next) => {
                this.onSuccessSavePersonalEvaluate.emit();
                this.common.UI.multipleNotify(
                  'EVALUATE SUCCESS',
                  'success',
                  2000
                );
              },
              (error) => {
                if (error.error == 'QUARTER_EVALUATION_NOT_FOUND') {
                  this.common.UI.multipleNotify(
                    'QUARTER_EVALUATION_NOT_FOUND',
                    'error',
                    2000
                  );
                } else {
                  this.common.UI.multipleNotify(
                    'ASSESSMENT_DURATION',
                    'error',
                    2000
                  );
                }
              }
            );
          break;
      }
    }
  }
  //#endregion

  //#region SELECT BOX EVENT

  onValueChangeYearSelected(e) {
    if (e.value == e.previousValue) {
      return;
    }
    this.selectedYear = e.value;
    this.quarterDataSource = this.avaiableModel.quarterSource.filter(
      (t) => t.yearId === e.value
    );
    this.selectedQuarter = null;
    this.selectedProject = null;
    this.isDoneSelectQuarter = false;
  }

  onValueChangeQuarterSelected(e) {
    if (e.value == e.previousValue) {
      return;
    }
    this.selectedQuarter = e.value;
    this.projectDatasource = this.avaiableModel.projectSource.filter(
      (t) => t.quarterId === e.value && t.yearId === this.selectedYear
    );

    this.selectedProject = null;
    this.isDoneSelectQuarter = false;
  }

  onValueProjectSelected(e) {
    if (e.value == e.previousValue) {
      return;
    }
    if (e.value == null) {
      this.isDoneSelectQuarter = false;
      this.onShowSubmitButton.emit(false);
      return;
    }
    this.selectedProject = e.value;
    this.onGetQuarterEvaluation();
    this.onShowSubmitButton.emit(true);
  }

  onGetQuarterEvaluation() {
    if (
      this.selectedYear !== null &&
      this.selectedQuarter !== null &&
      this.selectedProject !== null
    ) {
      let projectId = this.avaiableModel.projectSource.find(
        (t) => t.id === this.selectedProject
      ).projectId;
      let quarter = this.avaiableModel.quarterSource.find(
        (t) => t.id === this.selectedQuarter
      ).value;
      let year = this.avaiableModel.yearSource.find(
        (t) => t.id === this.selectedYear
      ).value;

      this.quarterEvaluationId = this.avaiableModel.dataSource.find(
        (t) =>
          t.projectId == projectId &&
          t.quarter.toString() == quarter &&
          t.year.toString() == year
      ).id;
      this.onInitData();
      this.isDoneSelectQuarter = true;
    }
  }

  //#endregion
}

@NgModule({
  imports: [
    BrowserModule,
    DxFormModule,
    DxButtonModule,
    DxPopupModule,
    DxTextAreaModule,
    DxValidatorModule,
    DxSelectBoxModule,
    DxBoxModule,
  ],
  exports: [EvaluationUserDetailComponent],
  declarations: [EvaluationUserDetailComponent],
  bootstrap: [EvaluationUserDetailComponent],
})
export class EvaluationUserDetailModule {}

enum EnumPersonalEvaluateFormState {
  CREATE = 1,
  EDIT = 2,
}
