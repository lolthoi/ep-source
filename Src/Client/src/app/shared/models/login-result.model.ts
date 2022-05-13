import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})

export class LoginResultModel {
    successful: boolean;
    token: string;
    firstName: string;
    lastName: string;
    fullName: string;
}