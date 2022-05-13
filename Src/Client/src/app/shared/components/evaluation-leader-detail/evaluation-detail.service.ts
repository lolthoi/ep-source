import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
const mainUrl = '/Evaluation';
@Injectable({
  providedIn: 'root'
})
export class EvaluationDetailService {
  getEvaluationByQuarter(params: HttpParams): any {
    return this.http.get(mainUrl, { params });
  }

  constructor(private http: HttpClient) { }
}
