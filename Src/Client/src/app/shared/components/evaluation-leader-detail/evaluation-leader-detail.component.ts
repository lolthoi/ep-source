import { HttpParams } from '@angular/common/http';
import { EvaluationDetailService } from './evaluation-detail.service';
import { Service } from './../../../pages/criterias/service';
import { Component, OnInit, NgModule, Input } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonService } from './../../services/common.service';
import DataSource from 'devextreme/data/data_source';
import {
  DxDataGridModule,
  DxScrollViewModule,
  DxTreeListModule,
  DxTextBoxModule,
  DxValidationSummaryModule,
  DxValidatorModule,
  DxNumberBoxModule,
} from 'devextreme-angular';

@Component({
  selector: 'app-evaluation-leader-detail',
  templateUrl: './evaluation-leader-detail.component.html',
  styleUrls: ['./evaluation-leader-detail.component.scss'],
})
export class EvaluationLeaderDetailComponent implements OnInit {
  @Input() id: any;
  @Input() quarterEvaluationId: any;
  @Input() component = {};
  treeListComp: any;
  textboxComp: any;
  validationComp: any;
  changeList = [];
  dataSource: DataSource;
  isDisableTextbox = true;

  privateComp = {
    getDataSource: () => {
      return this.changeList;
    },
    onClickEditEvaluation: () => {
      this.isDisableTextbox = false;
      setTimeout(() => {
        if (this.textboxComp != null) {
          this.textboxComp.focus();
        }
      }, 500);
    },
    onClickCloseEvaluation: () => {
      this.isDisableTextbox = true;
    },
    onReloadResource: () => {
      this.textboxComp = null;
      this.dataSource.reload();
    },
    onInitData: (id: any) => {},
  };

  constructor(
    private service: EvaluationDetailService,
    private criteriaService: Service,
    private common: CommonService
  ) {}

  onRowPrepared = (e: any) => {
    if (e.rowType === 'data' && e.data.typeId === null) {
      e.rowElement.style.backgroundColor = '#ecffd9';
      e.rowElement.style.fontWeight = 'bold';
    }
  };

  expandAllRow = () => {
    if (this.treeListComp != null && this.treeListComp != undefined)
      this.treeListComp.forEachNode((e) => {
        this.treeListComp.expandRow(e.key);
      });
  };

  onInitTreeList = (e: any) => {
    this.treeListComp = e.component;
  };

  onChangePoint = (e: any, data: any) => {
    if (data.point === '' || data.point === null) {
      return;
    }
    const existItem = this.changeList.filter((x) => x.id === data.id);
    if (existItem.length > 0) {
      existItem[0].point = e.value;
      return;
    }

    const item = {
      id: data.id,
      point: e.value,
    };
    this.changeList.push(item);
  };
  onInitTextbox = (e: any) => {
    if (this.textboxComp == null) {
      this.textboxComp = e.component;
    }
  };
  focusTextbox = () => {
    this.textboxComp.focus();
  };

  ngOnInit(): void {
    Object.assign(this.component, this.privateComp);
    if (this.quarterEvaluationId === undefined) {
      this.quarterEvaluationId = this.common.getGuidEmpty();
    }
    this.dataSource = new DataSource({
      load: async (options: any) => {
        const params = new HttpParams().set(
          'quarterId',
          this.quarterEvaluationId
        );
        const data = await this.service
          .getEvaluationByQuarter(params)
          .toPromise();
        return data;
      },
      onChanged: () => {
        this.expandAllRow();
      },
    });
  }
}

@NgModule({
  imports: [
    BrowserModule,
    DxTreeListModule,
    DxTextBoxModule,
    DxValidatorModule,
    DxScrollViewModule,
    DxValidationSummaryModule,
    DxNumberBoxModule,
    DxDataGridModule,
  ],
  exports: [EvaluationLeaderDetailComponent],
  declarations: [EvaluationLeaderDetailComponent],
  bootstrap: [EvaluationLeaderDetailComponent],
})
export class EvaluationLeaderDetailModule {}
