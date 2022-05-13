export class CriteriaTemplateModel {
  id: string;
  criteriaTypeTemplateId: string;
  evaluationTemplateId: string;
  criteriaStoreId: string;
  criteriaTypeStoreId: string;
  orderNo: number;

  constructor(init?: Partial<CriteriaTemplateModel>) {
    Object.assign(this, init);
  }
}
