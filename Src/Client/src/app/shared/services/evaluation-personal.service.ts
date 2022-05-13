import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { DataSourcePersonalEvaluateModel } from "../models/datasource-personal-evaluate.model";
import { EvaluationPersonalModel } from "../models/evaluation-personal.model";
import { UserEvaluationModel } from "../models/evaluation-user.model";

const apiUrl = {
    urlGetAllByPerson: `/Evaluation/GetAllByPerson`,
    urlGetQuarterEvaluationId: `/UserQuarterEvaluation`,
    baseUrl: `/UserQuarterEvaluation`,
    userBaseUrl: `/user`,
    urlGetAvaiableQuarterEvaluations: `/UserQuarterEvaluation`
}

@Injectable({
    providedIn: 'root'
})
export class EvaluationPersonalService {

    evaluationPersonalModels: EvaluationPersonalModel[];
    constructor(
        private httpClient: HttpClient
    ) {

    }
    public getEvaluationPersonals(startDate: number, endDate: number, projectId: number) {
        return this.httpClient.get<EvaluationPersonalModel[]>(`${apiUrl.urlGetAllByPerson}?startYear=${startDate}&endYear=${endDate}&projectId=${projectId}`);
    }

    // public getQuarterEvaluationIdByProjectId(userid: string, projectId: string) {
    //     return this.httpClient.get<QuarterEvaluationIdModel>(`${apiUrl.urlGetQuarterEvaluationId}/user/${userid}/project/${projectId}`);
    // }

    public getAvaiableQuarterEvaluations(userId: string) {
        return this.httpClient.get<DataSourcePersonalEvaluateModel>(`${apiUrl.urlGetAvaiableQuarterEvaluations}/user/${userId}`);
    }

    public createUserQuarterEvaluation(model: UserEvaluationModel) {
        return this.httpClient.post<UserEvaluationModel>(`${apiUrl.baseUrl}`, model);
    }

    public updateUserQuarterEvaluation(model: UserEvaluationModel) {
        return this.httpClient.put<UserEvaluationModel>(`${apiUrl.baseUrl}`, model);
    }
}