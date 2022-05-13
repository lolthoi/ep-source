import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { UserEvaluationModel } from "../models/evaluation-user.model";

const apiUrl = '/UserQuarterEvaluation'

@Injectable({
    providedIn: 'root',
})
export class UserEvaluationService {
    constructor(private router: Router, private httpClient: HttpClient) {
    }

    //GET

    getById(id: string) {
        return this.httpClient.get<UserEvaluationModel>(apiUrl + '/' + id);
    }

    //POST

    //PUT

    //DELETE

}
