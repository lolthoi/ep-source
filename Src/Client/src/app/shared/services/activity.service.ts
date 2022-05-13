import { ActivityModel } from './../models/activity.model';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

const apiUrl = '/Activity';
@Injectable({
  providedIn: 'root',
})
export class ActivityService {
  constructor(private http: HttpClient) {}
  public Create(model: ActivityModel): Observable<ActivityModel> {
    return this.http.post<ActivityModel>(apiUrl, model);
  }
  public Update(model: ActivityModel): Observable<ActivityModel> {
    return this.http.put<ActivityModel>(apiUrl, model);
  }
  public Delete(id: string): Observable<ActivityModel> {
    return this.http.delete<ActivityModel>(apiUrl + `/${id}`);
  }
}
