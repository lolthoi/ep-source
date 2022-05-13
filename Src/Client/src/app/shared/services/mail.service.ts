import { QuarterEvaluationModel } from './../models/quarter-evaluation.model';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

const apiUrl = '/Mail';
const urlEvaluation = '/GetAllEvaluation';

@Injectable({
  providedIn: 'root',
})
export class MailService {
  constructor(private http: HttpClient) {}

  public sendMailAsync(year: number, quarter: number) {
    let url = `/year` + `/${year}` + `/quarter` + `/${quarter}`;
    return this.http.get(apiUrl + url);
  }
  public deleteGeneratedEvaluation() {
    return this.http.delete(apiUrl);
  }

  public getAllEvaluation(): Observable<QuarterEvaluationModel[]> {
    return this.http.get<QuarterEvaluationModel[]>(apiUrl + urlEvaluation);
  }
}
