import { HttpParams } from '@angular/common/http';
import { AuthService } from './../../shared/services/auth.service';
import { Component, OnInit, NgModule, Input } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { EvaluationLeaderService } from './evaluation-leader-service';
import { CommonService } from './../../shared/services/common.service';
import { EvaluationDetailModule } from './../../shared/components/evaluation-detail/evaluation-detail.component';
import { ProjectService } from '../../shared/services/project.service';
import { ProjectRolesEnum } from '../../shared/models/user-app.model';
import { AppRolesEnum } from 'src/app/shared/models/user-app.model';
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
  selector: 'app-evaluation-leader',
  templateUrl: './evaluation-leader.component.html',
  styleUrls: ['./evaluation-leader.component.scss'],
})
export class EvaluationLeaderComponent implements OnInit {
  currentUserId: any;
  capValue1 = 'Quarter 1';
  capValue2 = 'Quarter 2';
  capValue3 = 'Quarter 3';
  capValue4 = 'Quarter 4';
  dataSource: any;
  projectId: any;
  projectItems = [];
  gridComp = null;
  selectBoxComp = null;
  isAllowEdit = false;
  quarterEvaluationId: any;
  component = {
    getDataSource: null,
    onShowPopUp: null,
    onRefreshGrid: () => {
      this.init();
    },
  };

  // tslint:disable-next-line:max-line-length
  constructor(
    private service: EvaluationLeaderService,
    private common: CommonService,
    private projectService: ProjectService,
    private authService: AuthService
  ) {
    this.projectService.GetProjectOfLeader().subscribe(
      (responeseData) => {
        this.projectItems = responeseData;
      },
      (error) => {
        this.common.UI.multipleNotify(error.error, 'error', 2000);
      }
    );
  }

  ngOnInit(): void {
    this.currentUserId = this.authService.getUserValue.id;
    this.init();
  }
  init = () => {

    this.service.getEvaluationForLeader(this.currentUserId).subscribe(
      (result: any) => {
        this.dataSource = result;
        if (this.projectId != null) {
          this.dataSource = this.dataSource.filter(
            (x) => x.projectId === this.projectId
          );
        }
        this.onSetRowTitle(result);
      },
      (err: any) => {
        this.common.UI.multipleNotify('Load data fail!!!', 'error', 2000);
      }
    );
  };
  onSetRowTitle = (data: any) => {
    if (data.length > 0) {
      const item = data[0];
      this.capValue1 = item.value1.quarter + '/' + item.value1.year;
      this.capValue2 = item.value2.quarter + '/' + item.value2.year;
      this.capValue3 = item.value3.quarter + '/' + item.value3.year;
      this.capValue4 = item.value4.quarter + '/' + item.value4.year;
    }
  };
  onCellClick = (e: any) => {
    let isEnableClick = false;
    if (e.rowType === 'data' && e.columnIndex === 1) {
      isEnableClick = true;
    }
    this.onHandlerCellClick(e.data, e.columnIndex);
    if (isEnableClick && (e.value === '' || e.value === null)) {
      console.log('add');
    } else if (isEnableClick && e.value !== null) {
      console.log('edit');
    } else {
      return;
    }
  };
  allowAddSubmit = (e: any) => {
    return false;
  };
  onInitGrid = (e: any) => {
    this.gridComp = e.component;
  };
  onToolbarPreparing = (e: any) => {
    e.toolbarOptions.items.unshift({
      location: 'before',
      locateInMenu: 'auto',
      widget: 'dxSelectBox',
      template: 'myToolbarTemplate',
    });
  };
  onInitSelectBox = (e: any) => {
    this.selectBoxComp = e.component;
  };
  onChanged = (e: any) => {
    this.projectId = e.selectedItem == null ? null : e.selectedItem.id;
    this.component.onRefreshGrid();
  };
  onHandlerCellClick = (data: any, valueIndex: number) => {

    const currentUser = this.authService.getUser;
    const value = data['value' + (valueIndex - 2)];
    if (!value) {
      return;
    }
    this.quarterEvaluationId = value.id;
    if (this.quarterEvaluationId === this.common.getGuidEmpty()) {
      this.common.UI.multipleNotify(
        'No Evaluation for this quarter!!!',
        'error',
        2000
      );
      return;
    }
    // tslint:disable-next-line:max-line-length
    const isleadOrAdmin =
      currentUser.projectRoles.filter(
        (x) =>
          x.projectId === data.projectId &&
          x.projectRoleId === ProjectRolesEnum.PM
      ).length > 0 || currentUser.appRole === AppRolesEnum.ADMINISTRATOR;
    // Queue this task for next event loops => function done, update input
    setTimeout(() => {
      this.component.onShowPopUp(
        isleadOrAdmin,
        value.score,
        value.quarter,
        value.year,
        value.createdDate,
        data.name,
        data.projectName
      );
    }, 0);
  };
}
@NgModule({
  imports: [
    DxDataGridModule,
    DxButtonModule,
    DxTextBoxModule,
    DxCheckBoxModule,
    DxPopupModule,
    DxTextAreaModule,
    BrowserModule,
    DxSelectBoxModule,
    DxValidatorModule,
    DxValidationSummaryModule,
    EvaluationDetailModule,
    DxScrollViewModule,
  ],
  declarations: [EvaluationLeaderComponent],
  bootstrap: [EvaluationLeaderComponent],
})
export class EvaluationLeaderModule {}
