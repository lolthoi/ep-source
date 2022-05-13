
export class PaginationModel<T> {
    page: number;
    Count: number;
    totalPages: number;
    totalCount: number;
    items: T[];
}