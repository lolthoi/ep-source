import { Component, NgModule, OnInit, ViewChild } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {
  DxButtonModule,
  DxDataGridModule,
  DxPopupModule,
} from 'devextreme-angular';
import {
  EvaluationTemplateFormComponent,
  EvaluationTemplateFormModule,
  EvaluationTemplateFormViewModel,
  FormState,
} from 'src/app/shared/components/evaluation-template-form/evaluation-template-form.component';
import { CriteriaTypeTemplateModel } from 'src/app/shared/models/criteria-type-template.model';
import { EvaluationTemplateViewModel } from 'src/app/shared/models/evaluation-template-view.model';
import { AuthService } from 'src/app/shared/services';
import { EvaluationTemplateService } from 'src/app/shared/services/evaluation-template.service';
import { PositionService } from 'src/app/shared/services/position.service';

@Component({
  selector: 'app-evaluation-template',
  templateUrl: './evaluation-template.component.html',
  styleUrls: ['./evaluation-template.component.scss'],
})
export class EvaluationTemplateComponent implements OnInit {
  isAdminRole = false;
  positions = [];
  positionsForCreate = [];
  dataSource: EvaluationTemplateViewModel[];
  criteriaType: CriteriaTypeTemplateModel[];
  @ViewChild(EvaluationTemplateFormComponent)
  evalutionTemplateFormComponent: EvaluationTemplateFormComponent;
  currentModel: EvaluationTemplateFormViewModel =
    new EvaluationTemplateFormViewModel();
  constructor(
    private evaluationService: EvaluationTemplateService,
    private authService: AuthService,
    private position: PositionService
  ) {
    evaluationService.getAllTemplate().subscribe(
      (next) => {
        this.dataSource = next;
      },
      (error) => {}
    );
    this.selectListPosition();
    this.isAdminRole = this.authService.isRoleAdministrator;
  }

  onToolbarPreparing(e) {
    e.toolbarOptions.items.unshift({
      location: 'after',
      widget: 'dxButton',
      options: {
        icon: 'add',
        width: 'auto',
        text: 'Add',
        stylingMode: 'contained',
        type: 'success',
        visible: this.isAdminRole,
        onClick: this.onOpenPopupADD.bind(this),
      },
    });
  }
  onOpenPopupADD(): void {
    this.initPositionsForCreate();
    this.currentModel.state = FormState.CREATE;
    this.currentModel.data = new EvaluationTemplateViewModel();
    this.currentModel.data.name = '';
    this.currentModel.data.positionId = null;
    this.currentModel.data.criteriaTemplateViewModels = [];

    this.evalutionTemplateFormComponent.open();
  }

  opOpenPopupDetail(e, data): void {
    this.initPositionsForCreate();

    var item = this.positions.filter((x) => x.id == data.data.positionId);
    if (item != null && item.length > 0) {
      this.positionsForCreate.push(item[0]);
    }
    this.positionsForCreate.sort((positionA, positionB) =>
      positionA.name > positionB.name ? 1 : -1
    );
    this.currentModel.state = FormState.DETAIL;
    this.currentModel.data = new EvaluationTemplateViewModel(data.data);
    this.evalutionTemplateFormComponent.open();
  }
  onRefreshGrid() {
    this.selectListPosition();
    this.evaluationService.getAllTemplate().subscribe(
      (next) => {
        this.dataSource = next;
      },
      (error) => {}
    );
  }

  selectListPosition() {
    this.position.getPositionsForTemplate().subscribe(
      (data) => {
        this.positions = data;
      },
      (error) => {}
    );
  }
  initPositionsForCreate(): void {
    this.positionsForCreate = this.positions;
    var positionIdExits = [
      ...new Set(this.dataSource.map((item) => item.positionId)),
    ];
    if (positionIdExits.length > 0) {
      positionIdExits.forEach((item) => {
        this.positionsForCreate = this.positionsForCreate.filter(
          (x) => x.id !== item
        );
      });
    }
  }
  ngOnInit(): void {}
}
@NgModule({
  imports: [
    BrowserModule,
    DxDataGridModule,
    DxButtonModule,
    DxPopupModule,

    EvaluationTemplateFormModule,
  ],
  declarations: [EvaluationTemplateComponent],
  bootstrap: [EvaluationTemplateComponent],
})
export class EvaluationTemplateModule {}
