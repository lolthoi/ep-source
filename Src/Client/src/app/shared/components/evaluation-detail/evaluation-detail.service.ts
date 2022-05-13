import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
const mainUrl = '/Evaluation';
const mainUrlLeder = '/Evaluation/leader';
@Injectable({
  providedIn: 'root'
})
export class EvaluationDetailService {
  createQuarterEvaluation(quarterId: any, model: any): any {
    const url = mainUrl + '?quarterId=' + quarterId;
    return this.http.post(url, model);
  }
  constructor(private http: HttpClient) { }
}
