import { TimeSheetReportUserActivityModel } from './../models/time-sheet-report-user-activity.model';
import { FilterParamUserRecord } from './../models/filter-param-user-record.model';
import { TimeSheetReportUserModel } from './../models/time-sheet-report-user.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FilterParamUserActivity } from '../models/time-sheet-report-user-activity.model';

const apiUrl = '/TimeSheetReportDetail';

@Injectable({
  providedIn: 'root',
})
export class TimeSheetReportUserService {
  constructor(private http: HttpClient) {}

  public GetAll(
    model: FilterParamUserRecord
  ): Observable<TimeSheetReportUserModel[]> {
    return this.http.post<TimeSheetReportUserModel[]>(
      apiUrl + '/GetAllUserRecord',
      model
    );
  }
  public GetAllUserActivity(
    model: FilterParamUserActivity
  ): Observable<TimeSheetReportUserActivityModel[]> {
    return this.http.post<TimeSheetReportUserActivityModel[]>(
      apiUrl + '/GetAllUserActivityRecord',
      model
    );
  }
}
