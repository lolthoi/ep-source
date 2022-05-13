import { TimeSheetRecordModel } from './../models/time-sheet-record.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { WorkSpaceSettingModel } from '../models/work-space-setting.model';

const apiUrl = {
  timeSheetRecordBaseUrl: '/timesheetrecord',
  activityRecordViewModel: '/activity/currentuser',
  activityRecordsModel: '/Activity/activityRecordViewModel',
  wsSettingUrl: '/WorkSpaceSetting',
};

@Injectable({
  providedIn: 'root',
})
export class TimeSheetRecordService {
  constructor(private router: Router, private httpClient: HttpClient) {}

  //Get WorkSpace setting value
  public getWsSetting(): Observable<WorkSpaceSettingModel> {
    return this.httpClient.get<WorkSpaceSettingModel>(apiUrl.wsSettingUrl);
  }

  //GETALL
  public getAll(params) {
    return this.httpClient.get(apiUrl.timeSheetRecordBaseUrl, { params });
  }

  //CREATE
  public add(entity: TimeSheetRecordModel) {
    return this.httpClient.post<TimeSheetRecordModel>(
      apiUrl.timeSheetRecordBaseUrl,
      entity
    );
  }

  //UPDATE
  public edit(entity: TimeSheetRecordModel) {
    return this.httpClient.put<TimeSheetRecordModel>(
      apiUrl.timeSheetRecordBaseUrl,
      entity
    );
  }
  public editReturnResponse(entity: TimeSheetRecordModel) {
    return this.httpClient
      .put<TimeSheetRecordModel>(apiUrl.timeSheetRecordBaseUrl, entity)
      .toPromise()
      .then((response: any) => response)
      .catch((error) => error);
  }

  //DELETE
  public delete(id) {
    return this.httpClient.delete(apiUrl.timeSheetRecordBaseUrl + `/` + id);
  }

  //HELPER FUNCTION
  public getAllTaskId() {
    return this.httpClient.get(apiUrl.activityRecordViewModel);
  }

  public activityRecordsModel() {
    return this.httpClient.get(apiUrl.activityRecordsModel);
  }

  public convertListToKeyValuePairObject(array: TimeSheetRecordModel[]) {
    //convert array to object map key/value pair
    let map = array.reduce(function (accumulator, currentValue) {
      let startTime = new Date(Date.parse(currentValue.startTime.toString()));
      let groupNameString = new Date(
        startTime.getFullYear(),
        startTime.getMonth(),
        startTime.getDate()
      ).toLocaleDateString('en-US');
      if (accumulator[groupNameString] == null)
        accumulator[groupNameString] = [];
      accumulator[groupNameString].push(currentValue);
      return accumulator;
    }, {});

    //sort by date key des
    let result = Object.keys(map)
      .sort(function (a, b) {
        let dateA = new Date(a);
        let dateB = new Date(b);
        return dateA < dateB ? -1 : 1;
      })
      .reduce(function (acc, cur) {
        let obj = {};
        let groupName = cur.toString();
        obj[groupName] = map[groupName];
        // return {...acc,...obj};
        return Object.assign(obj, acc);
      }, {});
    return result;
  }

  public parseStringTimeZoneToDate(str_date) {
    return new Date(Date.parse(str_date));
  }

  public calculateDiffMs(startDateString, endDateString) {
    let startTime = new Date(Date.parse(startDateString.toString()));
    let endTime = new Date(Date.parse(endDateString.toString()));
    var diff = endTime.getTime() - startTime.getTime();
    return diff;
  }

  public msToTime(s) {
    var ms = s % 1000;
    s = (s - ms) / 1000;
    var secs = s % 60;
    s = (s - secs) / 60;
    var mins = s % 60;
    var hrs = (s - mins) / 60;

    let hrsString = hrs > 9 ? hrs.toString() : '0' + hrs.toString();
    let minString = mins > 9 ? mins.toString() : '0' + mins.toString();

    return hrsString + ':' + minString /* + ':' + secs + '.' + ms*/;
  }
  //HELPER FUNCTION
}
