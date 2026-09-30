// هر عملیات ویرایشگر یک «فرمان» با execute و undo است
class InsertText {
  constructor(document, position, text) {
    Object.assign(this, { document, position, text });
  }
  execute() {
    const { content } = this.document;
    this.document.content = content.slice(0, this.position) + this.text + content.slice(this.position);
  }
  undo() {
    const { content } = this.document;
    this.document.content = content.slice(0, this.position) + content.slice(this.position + this.text.length);
  }
}

class History {
  #done = [];
  #undone = [];

  run(command) {
    command.execute();
    this.#done.push(command);
    this.#undone.length = 0; // فرمان جدید، تاریخچهٔ redo را باطل می‌کند
  }
  undo() {
    const command = this.#done.pop();
    if (command) {
      command.undo();
      this.#undone.push(command);
    }
  }
  redo() {
    const command = this.#undone.pop();
    if (command) {
      command.execute();
      this.#done.push(command);
    }
  }
}

const doc = { content: 'سلام' };
const commands = new History();
commands.run(new InsertText(doc, 4, ' دنیا'));
commands.run(new InsertText(doc, 9, '!'));
console.log(doc.content); // سلام دنیا!
commands.undo();
console.log(doc.content); // سلام دنیا
commands.undo();
commands.redo();
console.log(doc.content); // سلام دنیا
