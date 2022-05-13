import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ActivityModel } from '../models/activity.model';
import { PaginationModel } from '../models/pagination.model';
import { ActivityRecordViewModel } from '../models/time-sheet-record.model';
import { TimeSheetReportDetailModel } from '../models/time-sheet-report-detail-model';
import { TimeSheetReportDetailRouterModel } from '../models/time-sheet-report-detail-router.model';

const apiUrl = '/TimeSheetReportDetail/GetAllRecordPaging';

@Injectable({
  providedIn: 'root',
})
export class TimeSheetReportDetailService {
  constructor(private router: Router, private httpClient: HttpClient) {}

  public getAllRecordPaging(model: TimeSheetReportDetailRouterModel) {
    var params = new HttpParams();
    params = params.append('page', model.page.toString());
    params = params.append('pageSize', model.pageSize.toString());
    params = params.append('startDate', this.formatDate(model.startDate));
    params = params.append('endDate', this.formatDate(model.endDate));
    if (model.userIds != null && model.userIds != undefined) {
      if (model.userIds.length > 0) {
        model.userIds.forEach((item) => {
          params = params.append('userIds', item.toString());
        });
      }
    }
    if (model.projectIds != null && model.projectIds != undefined) {
      if (model.projectIds.length > 0) {
        model.projectIds.forEach((item) => {
          params = params.append('projectIds', item.toString());
        });
      }
    }
    if (
      model.tSAcitivityGroupIds != null &&
      model.tSAcitivityGroupIds != undefined
    ) {
      if (model.tSAcitivityGroupIds.length > 0) {
        model.tSAcitivityGroupIds.forEach((item) => {
          params = params.append('tSAcitivityGroupIds', item.toString());
        });
      }
    }
    if (model.taskIds != null && model.taskIds != undefined) {
      if (model.taskIds.length > 0) {
        model.taskIds.forEach((item) => {
          params = params.append('taskIds', item.toString());
        });
      }
    }
    return this.httpClient.get<PaginationModel<TimeSheetReportDetailModel>>(
      apiUrl,
      { params: params }
    );
  }

  public getListActivityName(groupIds: string[]) {
    return this.httpClient.post<ActivityRecordViewModel[]>(
      '/Activity/GetListActivityName',
      groupIds
    );
  }

  public getActivityForReportProject(groupIds: string[]) {
    return this.httpClient.post<ActivityRecordViewModel[]>(
      '/Activity/ActivityForReportProject',
      groupIds
    );
  }

  public getActivityByUserId(id: number) {
    return this.httpClient.get<ActivityRecordViewModel[]>(
      '/Activity/User/' + `${id}`
    );
  }

  public formatDate(date) {
    var d = new Date(date),
      month = '' + (d.getMonth() + 1),
      day = '' + d.getDate(),
      year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [year, month, day].join('-');
  }
}
