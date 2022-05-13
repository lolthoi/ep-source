import { Criteria } from './../../../pages/criterias/service';
import { HttpParams } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  NgModule,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {
  DxButtonModule,
  DxCheckBoxModule,
  DxDataGridModule,
  DxFormComponent,
  DxFormModule,
  DxPopupModule,
  DxScrollViewModule,
  DxTextBoxModule,
  DxTreeListModule,
  DxValidatorModule,
} from 'devextreme-angular';
import { Service } from 'src/app/pages/criterias/service';
import { PositionModel } from '../../models/position.model';
import { CommonService } from '../../services/common.service';
import { EvaluationTemplateService } from '../../services/evaluation-template.service';
import {
  CriteriaTemplateViewModel,
  EvaluationTemplateViewModel,
} from '../../models/evaluation-template-view.model';

@Component({
  selector: 'app-evaluation-template-form',
  templateUrl: './evaluation-template-form.component.html',
  styleUrls: ['./evaluation-template-form.component.scss'],
})
export class EvaluationTemplateFormComponent implements OnInit {
  @Input() model: EvaluationTemplateFormViewModel;
  @Input() positions: PositionModel[];
  @Output() onRefreshGrid = new EventEmitter<void>();
  @ViewChild(DxFormComponent, { static: false }) myform: DxFormComponent;
  isAdminRole = false;
  currentTemplate: EvaluationTemplateViewModel;
  criteriaTemplate: CriteriaTemplateViewModel[];
  criteria: Criteria[];
  formState = FormState;
  evTemplate: EvaluationTemplateViewModel;
  crTemplate: CriteriaTemplateViewModel[];
  popupConfirmDeleteVisible = false;
  popupVisible = false;
  isEnableDrag = false;
  popupTitle = '';
  popupCriteriaSelection = false;
  treeListDetailComp: any;
  criterias: any;
  treeListComp: any;
  selectedRowKeys: any;
  selectedData: any;
  toolBarTreeListComponent: any;
  toolBarTreeListEditComponent: any;
  isSaveOrder: boolean = false;
  evaluationTemplateViewModel = new EvaluationTemplateViewModel();
  listCriteriaTemplateViewModel: CriteriaTemplateViewModel[] = [];
  criteriaTemplateViewModel = new CriteriaTemplateViewModel();

  constructor(
    private templateService: EvaluationTemplateService,
    private common: CommonService,
    private service: Service
  ) {
    const params: HttpParams = new HttpParams().set('key', '');
    this.service.getCriterias(params).subscribe((result: any) => {
      this.criteria = result;
    });
  }
  enterEditFormButtonOptions = {
    icon: 'edit',
    text: 'Edit',
    onClick: (e) => {
      this.model.state = FormState.EDIT;
      this.toolBarTreeListComponent.option('visible', false);
      this.toolBarTreeListEditComponent.option('visible', true);
      this.myform.instance.getEditor('name').focus();
      this.isEnableDrag = false;
      this.open();
    },
  };
  onShowing = (e) => {
    if (
      this.model.state == FormState.EDIT ||
      this.model.state == FormState.CREATE
    ) {
      this.toolBarTreeListComponent.option('visible', false);
      this.toolBarTreeListEditComponent.option('visible', true);
    } else {
      this.toolBarTreeListComponent.option('visible', true);
      this.toolBarTreeListComponent.option('value', false);
      this.toolBarTreeListEditComponent.option('visible', false);
      this.isSaveOrder = false;
    }
  };
  deleteButtonOptions = {
    icon: 'trash',
    text: 'Delete',
    onClick: (e) => {
      this.popupConfirmDeleteVisible = true;
    },
  };
  confirmDeleteButtonOptions = {
    icon: 'save',
    text: 'Ok',
    onClick: (e) => {
      this.templateService.delete(this.currentTemplate.id).subscribe((next) => {
        this.popupConfirmDeleteVisible = false;
        this.popupVisible = false;
        this.onRefreshGrid.emit();
        this.common.UI.multipleNotify(
          'Delete Template Success',
          'Success',
          2000
        );
      });
    },
  };
  AddButtonOptions = {
    icon: 'save',
    text: 'Save',
    onClick: this.onSaveNewTemplate.bind(this),
  };
  onSaveNewTemplate(e) {
    this.myform.instance.getEditor('name').focus();
    var instance = this.myform.instance.validate();
    if (!instance.isValid) {
      return;
    }
    this.evaluationTemplateViewModel = {
      id: this.evaluationTemplateViewModel.id,
      name: this.currentTemplate.name,
      positionId: this.currentTemplate.positionId,
      criteriaTemplateViewModels: this.listCriteriaTemplateViewModel,
    };
    this.templateService.add(this.evaluationTemplateViewModel).subscribe(
      (res) => {
        this.common.UI.multipleNotify(
          'Create template success',
          'Success',
          4000
        );
        this.selectedData = null;
        this.selectedRowKeys = null;
        this.popupVisible = false;
        this.onRefreshGrid.emit();
      },
      (err) => {
        if (err.error === 'INVALID_TEMPLATE_REQUIRED_AT_LEASE_ONE_CRITERIA') {
          this.common.UI.multipleNotify(
            'There is no stored Criteria',
            'error',
            4000
          );
          this.common.UI.multipleNotify(
            'Create Criteria before create Evaluation template',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_NAME_NULL') {
          this.common.UI.multipleNotify(
            'Evaluation template name is required!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_NAME_MAX_LENGTH') {
          this.common.UI.multipleNotify(
            'Evaluation template name is too long!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_POSITION_NULL') {
          this.common.UI.multipleNotify(
            'Evaluation template position is required!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_DUPLICATED_POSITION') {
          this.common.UI.multipleNotify(
            'Evaluation template for this position already exists!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_NO_SELECTED_CRITERIA') {
          this.common.UI.multipleNotify(
            'Please select at least one criteria for this Evaluation Template.',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_LACK_OF_CRITERIA') {
          this.common.UI.multipleNotify(
            'The template contains some childless Criteria Types.',
            'error',
            4000
          );
          this.common.UI.multipleNotify(
            'Please add Criteria for Criteria Type before creating the Template.',
            'error',
            4000
          );
        }
      }
    );
  }
  editButtonOptions = {
    icon: 'save',
    text: 'Save',
    onClick: this.onSaveEditedTemplate.bind(this),
  };
  onSaveEditedTemplate(e) {
    this.onRefreshGrid.emit();
    var instance = this.myform.instance.validate();
    if (!instance.isValid) {
      return;
    }
    if (
      !this.listCriteriaTemplateViewModel ||
      this.listCriteriaTemplateViewModel.length === 0
    ) {
      this.listCriteriaTemplateViewModel = this.criteriaTemplate;
    }
    this.evaluationTemplateViewModel = {
      id: this.currentTemplate.id,
      name: this.currentTemplate.name,
      positionId: this.currentTemplate.positionId,
      criteriaTemplateViewModels: this.listCriteriaTemplateViewModel,
    };
    this.templateService.edit(this.evaluationTemplateViewModel).subscribe(
      (res) => {
        this.common.UI.multipleNotify('Edit template success', 'Success', 4000);
        this.popupVisible = false;
        this.selectedData = null;
        this.selectedRowKeys = null;
        this.onRefreshGrid.emit();
      },
      (err) => {
        if (err.error === 'TEMPLATE_NOT_FOUND') {
          this.common.UI.multipleNotify(
            'Evaluation template not exists!',
            'error',
            4000
          );
        }

        if (err.error === 'INVALID_TEMPLATE_NAME_NULL') {
          this.common.UI.multipleNotify(
            'Evaluation template name is required!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_NAME_MAX_LENGTH') {
          this.common.UI.multipleNotify(
            'Evaluation template name is too long!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_POSITION_NULL') {
          this.common.UI.multipleNotify(
            'Evaluation template position is required!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_DUPLICATED_POSITION') {
          this.common.UI.multipleNotify(
            'Evaluation template for this position already exists!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_NO_SELECTED_CRITERIA') {
          this.common.UI.multipleNotify(
            'Evaluation template requires at lease one Criteria to save!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_NOT_STORED_CRITERIA') {
          this.common.UI.multipleNotify(
            'Chosen Criteria is not existed!',
            'error',
            4000
          );
        }
        if (err.error === 'INVALID_TEMPLATE_LACK_OF_CRITERIA') {
          this.common.UI.multipleNotify(
            'The template contains some childless Criteria Types.',
            'error',
            4000
          );
          this.common.UI.multipleNotify(
            'Please add Criteria for Criteria Type before creating the Template.',
            'error',
            4000
          );
        }
      }
    );
  }
  closeButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.isSaveOrder = false;
      this.popupVisible = false;
      this.listCriteriaTemplateViewModel = null;
      this.selectedRowKeys = null;
    },
  };
  closeDeletePopupButtonOptions = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      this.popupConfirmDeleteVisible = false;
    },
  };
  open() {
    switch (this.model.state) {
      case FormState.CREATE:
        this.popupTitle = 'CREATE EVALUTION TEMPLATE';
        this.currentTemplate = this.model.data;
        if (this.myform) {
          this.myform.instance.resetValues();
        }
        this.criteriaTemplate = [];
        break;
      case FormState.DETAIL:
        this.popupTitle = 'DETAIL EVALUTION TEMPLATE';
        this.templateService.getbyId(this.model.data.id).subscribe(
          (result: any) => {
            this.currentTemplate = result;
            this.criteriaTemplate = result.criteriaTemplateViewModels;
          },
          (err: any) => {}
        );
        this.isSaveOrder = false;
        break;
      case FormState.EDIT:
        this.popupTitle = 'EDIT EVALUTION TEMPLATE';
        this.templateService.getbyId(this.model.data.id).subscribe(
          (result: any) => {
            this.currentTemplate = result;
            this.criteriaTemplate = result.criteriaTemplateViewModels;
          },
          (err: any) => {}
        );
        break;
    }
    this.popupVisible = true;
  }
  //Criteria Create Template
  ngOnInit(): void {
    this.InitCriteriaSelection();
  }
  InitCriteriaSelection() {
    const params: HttpParams = new HttpParams();
    this.service.getCriterias(params).subscribe(
      (result: any) => {
        this.criterias = result;
      },
      (err: any) => {
        this.common.UI.multipleNotify('Load data fail!!!', 'error', 2000);
      }
    );
  }
  onClickSelectCriteria(e) {
    switch (this.model.state) {
      case FormState.CREATE:
        this.popupCriteriaSelection = true;
        break;
      case FormState.EDIT:
        this.popupCriteriaSelection = true;
        this.selectedRowKeys = [];
        if (
          this.selectedData !== null &&
          this.selectedData !== undefined &&
          this.selectedData.length > 0
        ) {
          var listCriteria = this.selectedData.filter((i) => i.typeId !== null);
          listCriteria.forEach((e) => {
            if (e.criteriaTypeStoreId !== null) {
              this.selectedRowKeys.push(e.id);
            }
          });
        } else {
          this.criteriaTemplate.forEach((e) => {
            if (e.criteriaTypeStoreId !== null) {
              this.selectedRowKeys.push(e.criteriaStoreId);
            }
          });
        }
        break;
    }
  }
  onInitTreeList = (e: any) => {
    this.treeListComp = e.component;
  };

  onRowPrepared = (e: any) => {
    if (e.rowType === 'data' && e.data.typeId === null) {
      e.rowElement.style.backgroundColor = '#ecffd9';
      e.rowElement.style.fontWeight = 'bold';
    }
  };
  closeSelectionCriteria = {
    text: 'Cancel',
    icon: 'close',
    onClick: (e) => {
      if (!this.criteriaTemplate) {
        this.selectedRowKeys = [];
      } else {
        this.selectedRowKeys = [];
        var listCriteria = this.criteriaTemplate.filter(
          (i) => i.typeId !== null
        );
        listCriteria.forEach((e) => {
          this.selectedRowKeys.push(e.id);
        });
      }
      this.popupCriteriaSelection = false;
    },
  };
  saveSelectionCriteria = {
    text: 'Save',
    icon: 'save',
    onClick: (e) => {
      if (!this.selectedData || this.selectedData.length === 0) {
        this.common.UI.multipleNotify(
          'Select at least one criteria !',
          'error',
          4000
        );
      } else {
        this.listCriteriaTemplateViewModel = [];
        this.selectedData.forEach((e) => {
          if (e.typeId === null) {
            this.criteriaTemplateViewModel = {
              id: this.selectedData.id,
              templateId: null,
              typeId: null,
              description: e.description,
              orderNo: e.orderNo,
              name: e.name,
              criteriaStoreId: e.id,
              criteriaTypeStoreId: null,
            };
            this.listCriteriaTemplateViewModel.push(
              this.criteriaTemplateViewModel
            );
          } else {
            this.criteriaTemplateViewModel = {
              id: this.selectedData.id,
              templateId: null,
              typeId: null,
              description: e.description,
              orderNo: e.orderNo,
              name: e.name,
              criteriaStoreId: e.id,
              criteriaTypeStoreId: e.typeId,
            };
            this.listCriteriaTemplateViewModel.push(
              this.criteriaTemplateViewModel
            );
          }
        });
        this.criteriaTemplate = this.selectedData;
        this.popupCriteriaSelection = false;
      }
    },
  };
  onSelectionChanged(e: any) {
    this.selectedData = e.selectedRowsData;
    e.selectedRowsData.forEach((x) => {
      if (x.typeId !== null) {
        var criteriaType = new Criteria();
        this.criterias.forEach((e) => {
          if (e.id === x.typeId) {
            criteriaType = {
              id: x.typeId,
              typeId: null,
              name: e.name,
              description: e.description,
              orderNo: e.orderNo,
            };
          }
        });
        var existed = this.selectedData.filter((i) => i.id === criteriaType.id);
        if (existed !== null && existed.length === 0) {
          this.selectedData.push(criteriaType);
        }
      } else {
        var criteria = new Criteria();
        this.criterias.forEach((e) => {
          if (e.typeId === x.id) {
            criteria = {
              id: e.id,
              typeId: e.typeId,
              name: e.name,
              description: e.description,
              orderNo: e.orderNo,
            };
            var existed = this.selectedData.filter((i) => i.id === criteria.id);
            if (existed !== null && existed.length === 0) {
              this.selectedData.push(criteria);
            }
          }
        });
      }
    });
  }
  //End Criteria Create Template
  onInitTreeListDetail(e: any) {
    this.treeListDetailComp = e.component;
  }
  InitCriteriaDetail() {
    const id = this.currentTemplate.id;
    this.templateService.getbyId(id).subscribe(
      (result: any) => {
        this.currentTemplate = result;
        this.criteriaTemplate = result.criteriaTemplateViewModels;
      },
      (err: any) => {}
    );
  }
  onDragChange(e: any) {
    const visibleRows = e.component.getVisibleRows();
    const sourceNode = e.component.getNodeByKey(e.itemData.id);
    let targetNode = visibleRows[e.toIndex].node;

    while (targetNode && targetNode.data) {
      if (targetNode.data.orderNo === sourceNode.data.orderNo) {
        e.cancel = true;
        break;
      }
      targetNode = targetNode.parent;
    }
  }
  onToolbarPreparing = (e: any) => {
    e.toolbarOptions.items.unshift({
      location: 'before',
      locateInMenu: 'auto',
      template: 'myToolbarTemplate',
    });
  };

  onInitializedToolBarTreeListEditComponent = (e) => {
    this.toolBarTreeListEditComponent = e.component;
  };
  onInitializedToolBarTreeListComponent = (e: any) => {
    this.toolBarTreeListComponent = e.component;
  };

  onCheckBoxDragChange = (e: any) => {
    this.isEnableDrag = e.value;
    if (!e.value && e.previousValue && this.isSaveOrder == true) {
      const id = this.currentTemplate.id;
      const data = this.criteriaTemplate;
      this.isSaveOrder = false;

      this.common.UI.confirmBox(
        'Do you want to delete the record?',
        'Notice'
      ).then((result) => {
        if (result) {
          this.templateService.orderCriteria(id, data).subscribe(
            (resultData: any) => {
              this.InitCriteriaDetail();
            },
            (err: any) => {}
          );
        }
      });
    }
    return;
  };

  onReorder = (e: any) => {
    const visibleRows = e.component.getVisibleRows();
    const sourceData = e.itemData;
    const targetData = visibleRows[e.toIndex].data;
    if (e.dropInsideItem) {
      if (targetData.typeId !== null || e.itemData.typeId === null) {
        return;
      }
      if (sourceData.typeId !== null && sourceData.typeId !== targetData.id) {
        return;
      }
      e.itemData.typeId = targetData.id;
      e.component.refresh();
    } else {
      const sourceIndex = this.criteriaTemplate.indexOf(sourceData);
      let targetIndex = this.criteriaTemplate.indexOf(targetData);
      // cannot drop to chill
      if (sourceData.typeId === null && targetData.typeId !== null) {
        return;
      }
      // cannot drop chill to parrent
      if (sourceData.typeId !== null && targetData.typeId === null) {
        return;
      }
      // cannot drop chill to other parrent
      if (
        sourceData.typeId !== null &&
        targetData.typeId !== sourceData.typeId
      ) {
        return;
      }
      if (sourceData.typeId !== targetData.typeId) {
        sourceData.typeId = targetData.typeId;
        if (e.toIndex > e.fromIndex) {
          targetIndex++;
        }
      }
      this.criteriaTemplate.splice(sourceIndex, 1);
      this.criteriaTemplate.splice(targetIndex, 0, sourceData);
      this.isSaveOrder = true;
    }
  };
}

@NgModule({
  imports: [
    BrowserModule,
    DxDataGridModule,
    DxButtonModule,
    DxPopupModule,
    DxFormModule,
    DxTextBoxModule,
    DxValidatorModule,
    DxScrollViewModule,
    DxTreeListModule,
    DxCheckBoxModule,
  ],
  declarations: [EvaluationTemplateFormComponent],
  exports: [EvaluationTemplateFormComponent],
})
export class EvaluationTemplateFormModule {}

export class EvaluationTemplateFormViewModel {
  state: FormState;
  data: EvaluationTemplateViewModel;

  constructor(init?: Partial<EvaluationTemplateFormViewModel>) {
    Object.assign(this, init);
  }
}
export enum FormState {
  DETAIL,
  CREATE,
  EDIT,
}
