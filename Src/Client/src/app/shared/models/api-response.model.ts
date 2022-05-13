import { ErrorModel } from "./error.model";

export class ApiResponse<T> {
    data: T;
    error: ErrorModel;
}