import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivityGroupModel } from '../models/activity-group.model';

const apiUrl = '/ActivityGroup';

@Injectable({
  providedIn: 'root',
})
export class ActivityGroupService {
  constructor(private http: HttpClient) {}

  public GetAll(): Observable<ActivityGroupModel[]> {
    return this.http.get<ActivityGroupModel[]>(apiUrl);
  }

  public ActivityGroupTimeSheetReport() {
    return this.http.get<ActivityGroupModel[]>(
      apiUrl + '/ActivityGroupTimeSheetReport'
    );
  }

  public ActivityGroupProjectReport(): Observable<ActivityGroupModel[]> {
    return this.http.get<ActivityGroupModel[]>(
      apiUrl + '/ActivityGroupForReportProject'
    );
  }

  public Create(model: ActivityGroupModel): Observable<ActivityGroupModel> {
    return this.http.post<ActivityGroupModel>(apiUrl, model);
  }

  public Update(model: ActivityGroupModel): Observable<ActivityGroupModel> {
    return this.http.put<ActivityGroupModel>(apiUrl, model);
  }

  public GetById(id: string): Observable<ActivityGroupModel> {
    return this.http.get<ActivityGroupModel>(apiUrl + `/${id}`);
  }

  public Delete(id: string): Observable<ActivityGroupModel> {
    return this.http.delete<ActivityGroupModel>(apiUrl + `/${id}`);
  }
}
