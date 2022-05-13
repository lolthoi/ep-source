import { PositionModel } from './../models/position.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

const apiUrl = '/position'

@Injectable({
  providedIn: 'root'
})
export class PositionService {

  constructor(private router: Router, private httpClient: HttpClient) {
  }

  // GET
  public getPositions() {
    return this.httpClient.get<PositionModel[]>(apiUrl);
  }

  public getPositionsForTemplate() {
    return this.httpClient.get<PositionModel[]>(`${apiUrl}/PositionsForTemplate`)
  }
}
