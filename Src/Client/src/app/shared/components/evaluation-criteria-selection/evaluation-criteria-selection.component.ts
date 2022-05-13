import { Component, NgModule, OnInit } from '@angular/core';
import {
  DxButtonModule,
  DxDataGridModule,
  DxFormModule,
  DxPopupModule,
  DxScrollViewModule,
  DxTextBoxModule,
  DxValidatorModule,
  DxTreeListModule,
} from 'devextreme-angular';

@Component({
  selector: 'app-evaluation-criteria-selection',
  templateUrl: './evaluation-criteria-selection.component.html',
  styleUrls: ['./evaluation-criteria-selection.component.scss'],
})
export class EvaluationCriteriaSelectionComponent implements OnInit {
  selectionCriteria = '';
  popupVisible = false;
  constructor() {}

  ngOnInit(): void {}
}
@NgModule({
  imports: [
    DxButtonModule,
    DxDataGridModule,
    DxFormModule,
    DxPopupModule,
    DxScrollViewModule,
    DxTextBoxModule,
    DxValidatorModule,
    DxTreeListModule,
  ],
  declarations: [EvaluationCriteriaSelectionComponent],
  bootstrap: [EvaluationCriteriaSelectionComponent],
})
export class EvaluationCriteriaSelectionModule {}
