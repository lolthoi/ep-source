import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import {
  CriteriaTemplateViewModel,
  EvaluationTemplateViewModel,
} from '../models/evaluation-template-view.model';
import { EvaluationTemplateModel } from '../models/evaluation-template.model';
import { LoginModel } from '../models/login.model';

const apiUrl = {
  evalutionTemplateBaseUrl: '/EvaluationTemplate',
};
@Injectable({
  providedIn: 'root',
})
export class EvaluationTemplateService {
  loginModel = new LoginModel();
  constructor(private router: Router, private httpClient: HttpClient) {}

  //Get
  public getAllTemplate() {
    return this.httpClient.get<EvaluationTemplateViewModel[]>(
      apiUrl.evalutionTemplateBaseUrl
    );
  }
  public getbyId(id: string) {
    return this.httpClient.get<EvaluationTemplateViewModel>(
      apiUrl.evalutionTemplateBaseUrl + '/' + id
    );
  }

  //ADD
  public add(entity: EvaluationTemplateViewModel) {
    return this.httpClient.post<EvaluationTemplateViewModel>(
      apiUrl.evalutionTemplateBaseUrl,
      entity
    );
  }

  //EDIT
  public edit(entity: EvaluationTemplateViewModel) {
    return this.httpClient.put<EvaluationTemplateViewModel>(
      apiUrl.evalutionTemplateBaseUrl,
      entity
    );
  }

  //DELETE
  public delete(id: string) {
    return this.httpClient.delete<boolean>(
      apiUrl.evalutionTemplateBaseUrl + '/' + id
    );
  }
  public orderCriteria(id:string, data: CriteriaTemplateViewModel[])
  {
    const url = apiUrl.evalutionTemplateBaseUrl +'/' + id +  '/reorder';
    return this.httpClient.post(url,data);
  }
}
