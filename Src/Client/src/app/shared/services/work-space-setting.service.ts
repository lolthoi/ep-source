import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { WorkSpaceSettingModel } from '../models/work-space-setting.model';

const apiUrl = '/WorkSpaceSetting'

@Injectable({
  providedIn: 'root'
})
export class WorkSpaceSettingService {

  constructor(private router: Router, private httpClient: HttpClient) {
  }

  public getWorkSpaceSettings() {
    return this.httpClient.get<WorkSpaceSettingModel>(apiUrl);
  }

  public edit(entity: WorkSpaceSettingModel) {
    return this.httpClient.put<WorkSpaceSettingModel>(apiUrl, entity);
  }
}
