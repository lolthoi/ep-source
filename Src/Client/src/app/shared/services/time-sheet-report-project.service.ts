import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FilterParamProjectRecord } from '../models/filter-param-project-record.model';
import {
  FilterParamProjectActivity,
  TimeSheetReportProjectActivityModel,
} from '../models/time-sheet-report-project-activity.model';
import { TimeSheetReportProjectModel } from '../models/time-sheet-report-project.model';

const apiUrl = '/TimeSheetReportDetail';

@Injectable({
  providedIn: 'root',
})
export class TimeSheetReportProjectService {
  constructor(private http: HttpClient) {}

  public GetAll(
    model: FilterParamProjectRecord
  ): Observable<TimeSheetReportProjectModel[]> {
    return this.http.post<TimeSheetReportProjectModel[]>(
      apiUrl + '/GetAllProjectRecord',
      model
    );
  }
  public GetAllProjectActivity(
    model: FilterParamProjectActivity
  ): Observable<TimeSheetReportProjectActivityModel[]> {
    return this.http.post<TimeSheetReportProjectActivityModel[]>(
      apiUrl + '/GetAllProjectActivityRecord',
      model
    );
  }
}
