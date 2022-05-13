export class EvaluationPersonalModel {
    quarter: string;
    position: string;
    pointAverage: number;
    leader: string;
    project: string;
    quarterText: string

    constructor(init?: Partial<EvaluationPersonalModel>) {
        Object.assign(this, init);
    }
}