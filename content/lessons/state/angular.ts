import { Injectable, computed, signal } from '@angular/core';

type Status = 'pending' | 'paid' | 'shipped' | 'delivered' | 'cancelled';
type Action = 'pay' | 'ship' | 'deliver' | 'cancel';

const TRANSITIONS: Record<Status, Partial<Record<Action, Status>>> = {
  pending: { pay: 'paid', cancel: 'cancelled' },
  paid: { ship: 'shipped', cancel: 'cancelled' },
  shipped: { deliver: 'delivered' },
  delivered: {},
  cancelled: {},
};

@Injectable()
export class OrderStateMachine {
  private readonly state = signal<Status>('pending');

  readonly status = this.state.asReadonly();
  // قالب دکمه‌ها را فقط از روی همین لیست نشان می‌دهد؛ اکشن غیرمجاز اصلاً دکمه ندارد
  readonly allowedActions = computed(() => Object.keys(TRANSITIONS[this.state()]) as Action[]);

  dispatch(action: Action): void {
    const next = TRANSITIONS[this.state()][action];
    if (!next) throw new Error(`«${action}» در وضعیت «${this.state()}» مجاز نیست`);
    this.state.set(next);
  }
}

// قالب:
//   @for (action of machine.allowedActions(); track action) {
//     <button (click)="machine.dispatch(action)">{{ action }}</button>
//   }
