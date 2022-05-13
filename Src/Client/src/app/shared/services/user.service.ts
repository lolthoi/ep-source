import { LoginModel } from './../models/login.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { UserModel } from '../models/user.model';
import { ResetPasswordModel } from '../models/reset-password.model';
import { GoogleLoginModel } from '../models/google-login.model';

const apiUrl = {
  userBaseUrl: '/user',
};

@Injectable({
  providedIn: 'root',
})
export class UserService {
  loginModel = new LoginModel();

  constructor(private router: Router, private httpClient: HttpClient) {}

  public login(username, password) {
    this.loginModel.email = username;
    this.loginModel.password = password;
    this.loginModel.rememberMe = false;

    return this.httpClient.post(`/account`, this.loginModel);
  }

  // GET
  public getUsers(stringText) {
    let queryString =
      !stringText && stringText.length === 0 ? '' : '?searchText=${stringText}';
    return this.httpClient.get<UserModel[]>(apiUrl.userBaseUrl + queryString);
  }

  public getUserById(id: number) {
    return this.httpClient.get<UserModel>(apiUrl.userBaseUrl + '/' + id);
  }

  public getUsersForTimeSheetReport(projectIds: number[]) {
    return this.httpClient.post<UserModel[]>(
      apiUrl.userBaseUrl + '/UsersForTimeSheetReport',
      projectIds
    );
  }

  //POST

  public add(entity: UserModel) {
    return this.httpClient.post<UserModel>(apiUrl.userBaseUrl, entity);
  }

  async changedPassword(entity: ResetPasswordModel) {
    return this.httpClient.post<ResetPasswordModel>(
      '/User/ChangedPassword',
      entity
    );
  }

  //PUT

  public edit(entity: UserModel) {
    return this.httpClient.put<UserModel>(apiUrl.userBaseUrl, entity);
  }

  //DELETE

  public delete(id: number) {
    return this.httpClient.delete<boolean>(apiUrl.userBaseUrl + `/` + id);
  }

  // EXTERNAL LOGIN

  public externalLogin(googleTokenId: string) {
    var model = new GoogleLoginModel();
    model.googleTokenId = googleTokenId;

    return this.httpClient.post('/account/LoginExternalCallBack', model);
  }
}
