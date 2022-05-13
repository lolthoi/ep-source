export class EvaluationTemplateViewModel {
  id: string;
  name: string;
  positionId: number;
  criteriaTemplateViewModels: CriteriaTemplateViewModel[];
  constructor(init?: Partial<EvaluationTemplateViewModel>) {
    Object.assign(this, init);
  }
}

export class CriteriaTemplateViewModel {
  id: string;
  templateId: string;
  typeId: number;
  description: string;
  orderNo: number;
  name: string;
  criteriaStoreId: string;
  criteriaTypeStoreId: string;
  constructor(init?: Partial<CriteriaTemplateViewModel>) {
    Object.assign(this, init);
  }
}
