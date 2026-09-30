import { Component, computed, signal } from '@angular/core';

interface TrieNode {
  children: Map<string, TrieNode>;
  isWord: boolean;
}

const normalize = (text: string): string => text.trim().toLowerCase().replaceAll('ي', 'ی').replaceAll('ك', 'ک');

class Trie {
  private readonly root: TrieNode = { children: new Map(), isWord: false };

  insert(word: string): void {
    let node = this.root;
    for (const char of normalize(word)) {
      let next = node.children.get(char);
      if (!next) node.children.set(char, (next = { children: new Map(), isWord: false }));
      node = next;
    }
    node.isWord = true;
  }

  suggest(prefix: string, limit: number): string[] {
    const typed = normalize(prefix);
    let node: TrieNode | undefined = this.root;
    for (const char of typed) {
      node = node.children.get(char);
      if (!node) return [];
    }
    const results: string[] = [];
    const collect = (current: TrieNode, text: string): void => {
      if (results.length >= limit) return;
      if (current.isWord) results.push(text);
      for (const [char, child] of current.children) collect(child, text + char);
    };
    collect(node, typed);
    return results;
  }
}

@Component({
  selector: 'app-search-box',
  template: `
    <input type="search" role="combobox" aria-label="جست‌وجو" [value]="query()" (input)="onInput($event)" />
    <ul>
      @for (s of suggestions(); track s) {
        <li><button type="button" (click)="query.set(s)">{{ s }}</button></li>
      }
    </ul>
  `,
})
export class SearchBox {
  protected readonly query = signal('');

  // درخت فقط یک بار از فهرست محصولات ساخته می‌شود؛ هر کاراکتر تایپ‌شده فقط پیشوند را دنبال می‌کند
  private readonly trie = ((words: string[]) => {
    const t = new Trie();
    words.forEach((w) => t.insert(w));
    return t;
  })(['گوشی', 'گوشی سامسونگ', 'گوشی شیائومی', 'گوشواره', 'لپ‌تاپ']);

  protected readonly suggestions = computed(() => (this.query() ? this.trie.suggest(this.query(), 5) : []));

  protected onInput(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }
}
