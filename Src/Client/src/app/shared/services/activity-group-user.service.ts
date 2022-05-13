import { ActivityGroupUserModel } from './../models/activity-group-user.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

const apiUrl = '/ActivityGroupUser';
@Injectable({
  providedIn: 'root',
})
export class ActivityGroupUserService {
  constructor(private http: HttpClient) {}
  public Create(
    model: ActivityGroupUserModel
  ): Observable<ActivityGroupUserModel> {
    return this.http.post<ActivityGroupUserModel>(apiUrl, model);
  }
  public Update(
    model: ActivityGroupUserModel
  ): Observable<ActivityGroupUserModel> {
    return this.http.put<ActivityGroupUserModel>(apiUrl, model);
  }
  public GetById(id: string): Observable<ActivityGroupUserModel> {
    return this.http.get<ActivityGroupUserModel>(apiUrl + `/${id}`);
  }
  public Delete(id: string): Observable<ActivityGroupUserModel> {
    return this.http.delete<ActivityGroupUserModel>(apiUrl + `/${id}`);
  }
  public CheckManagerByUserId(id: number): Observable<ActivityGroupUserModel> {
    return this.http.get<ActivityGroupUserModel>(apiUrl + `/${id}`);
  }
}
