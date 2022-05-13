import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})

export class LoginModel {
    email: string;
    password:string;
    rememberMe: boolean;
}