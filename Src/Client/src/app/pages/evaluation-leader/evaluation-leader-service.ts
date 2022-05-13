import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
const mainUrl = '/Evaluation';
const mainUrlLeader = '/Evaluation/leader';
@Injectable({
  providedIn: 'root'
})
export class EvaluationLeaderService {
  getEvaluationForLeader(id: any): any {
    const url = mainUrlLeader + '/' + id;
    return this.http.get(url);
  }
  getEvaluationByQuarter(params: HttpParams): any {
    return this.http.get(mainUrl, { params });
  }

  constructor(private http: HttpClient) { }
}
