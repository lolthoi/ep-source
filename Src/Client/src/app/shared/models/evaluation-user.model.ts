export class UserEvaluationModel {
    id: string;
    quarterEvaluationId: string;
    NoteGoodThing: string;
    NoteBadThing: string;
    NoteOther: string;

    constructor(init?: Partial<UserEvaluationModel>) {
        Object.assign(this, init);
    }
}
