interface TrieNode {
  children: Map<string, TrieNode>;
  isWord: boolean;
}

const normalize = (text: string): string => text.trim().toLowerCase().replaceAll('ي', 'ی').replaceAll('ك', 'ک');
const createNode = (): TrieNode => ({ children: new Map(), isWord: false });

class Trie {
  private readonly root = createNode();

  insert(word: string): void {
    let node = this.root;
    for (const char of normalize(word)) {
      let next = node.children.get(char);
      if (!next) node.children.set(char, (next = createNode()));
      node = next;
    }
    node.isWord = true;
  }

  suggest(prefix: string, limit = 5): string[] {
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

const trie = new Trie();
['گوشی', 'گوشی سامسونگ', 'گوشی شیائومی', 'گوشواره', 'لپ‌تاپ', 'كتاب'].forEach((w) => trie.insert(w));
console.log(trie.suggest('گوش'));
console.log(trie.suggest('کتا')); // ['کتاب'] ← «ك» عربی هنگام درج به «ک» فارسی تبدیل شده
