import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ProjectUserModel } from "../models/project-user.model";
import { UserModel } from "../models/user.model";


const apiUrl = {
  urlProjectMemberCreate: `/Project`,
  urlProjectMemberEdit: '/Project',
  urlProjectMemberDelete: '/Project',
  urlProjectMemberGetAll: '/Project/{0}/ProjectUser',
  urlProjectMemberGetById: '/Project'
}


@Injectable({
  providedIn: 'root'
})
export class ProjectUserService {
  constructor(private httpClient: HttpClient) {

  }

  //#region  GET

  public GetProjectMember(projectId: number) {
    return this.httpClient.get<ProjectUserModel[]>(`/Project/${projectId}/ProjectUser`);
  }

  public GetTopFiveUserNotInProject(projectId: number, textSearch) {
    return this.httpClient.get<UserModel[]>
      (`/Project/ProjectUser/GetTopFiveUserNotInProject?projectId=${projectId}&searchText=${textSearch}`);
  }
  public GetProjectMemberById(projectId: number, projectUserId: string) {
    return this.httpClient.get<ProjectUserModel>(`/Project/${projectId}/ProjectUser/${projectUserId}`);
  }

  //#endregion

  //#region POST

  public add(projectId: number, userId: number) {
    return this.httpClient.post<ProjectUserModel>(`/Project/${projectId}/ProjectUser/${userId}`, []);
  }

  //#endregion

  //#region  UPDATE

  public edit(projectId: number, projectUserId: string, projectRoleId: number) {
    return this.httpClient.put<ProjectUserModel>(`/Project/${projectId}/ProjectUser?projectUserId=${projectUserId}&projectRoleId=${projectRoleId}`, []);
  }

  //#endregion

  //#region DELETE

  public delete(projectId: number, projectUserId: string) {
    return this.httpClient.delete<boolean>(`/Project/${projectId}/ProjectUser/${projectUserId}`);
  }

  //#endregion
}

