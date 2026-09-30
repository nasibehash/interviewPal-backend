interface Command {
  execute(): void;
  undo(): void;
}

class Cart {
  readonly items: string[] = [];
}

class AddItem implements Command {
  constructor(private readonly cart: Cart, private readonly item: string) {}
  execute(): void {
    this.cart.items.push(this.item);
  }
  undo(): void {
    this.cart.items.pop();
  }
}

class RemoveItem implements Command {
  private index = -1;
  constructor(private readonly cart: Cart, private readonly item: string) {}
  execute(): void {
    this.index = this.cart.items.indexOf(this.item);
    if (this.index >= 0) this.cart.items.splice(this.index, 1);
  }
  undo(): void {
    if (this.index >= 0) this.cart.items.splice(this.index, 0, this.item); // در همان جای قبلی
  }
}

class CommandHistory {
  private readonly done: Command[] = [];
  private readonly undone: Command[] = [];

  run(command: Command): void {
    command.execute();
    this.done.push(command);
    this.undone.length = 0;
  }
  undo(): void {
    const command = this.done.pop();
    if (command) {
      command.undo();
      this.undone.push(command);
    }
  }
  redo(): void {
    const command = this.undone.pop();
    if (command) {
      command.execute();
      this.done.push(command);
    }
  }
}

const cart = new Cart();
const commands = new CommandHistory();
commands.run(new AddItem(cart, 'گوشی'));
commands.run(new AddItem(cart, 'قاب'));
commands.run(new RemoveItem(cart, 'گوشی'));
console.log(cart.items); // ['قاب']
commands.undo();
console.log(cart.items); // ['گوشی', 'قاب'] ← در جای اصلی
