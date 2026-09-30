// یکسان‌سازی حروف فارسی/عربی و فاصله‌ها: «كتاب» و «کتاب» باید یکی حساب شوند
const normalize = (text) => text.trim().toLowerCase().replaceAll('ي', 'ی').replaceAll('ك', 'ک');

class Trie {
  root = { children: new Map(), isWord: false };

  insert(word) {
    let node = this.root;
    for (const char of normalize(word)) {
      if (!node.children.has(char)) node.children.set(char, { children: new Map(), isWord: false });
      node = node.children.get(char);
    }
    node.isWord = true;
  }

  // همهٔ کلمه‌هایی که با prefix شروع می‌شوند (حداکثر limit تا)
  suggest(prefix, limit = 5) {
    let node = this.root;
    const typed = normalize(prefix);
    for (const char of typed) {
      node = node.children.get(char);
      if (!node) return []; // هیچ کلمه‌ای با این پیشوند نیست
    }

    const results = [];
    const collect = (current, text) => {
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

console.log(trie.suggest('گوش')); // ['گوشی', 'گوشی سامسونگ', 'گوشی شیائومی', 'گوشواره']
console.log(trie.suggest('گوشی ').length); // 3 ← فاصلهٔ انتهایی حذف می‌شود
console.log(trie.suggest('لپ‌تاپ اپل')); // []
console.log(trie.suggest('کتا')); // ['کتاب'] ← «ك» عربی هنگام درج به «ک» فارسی تبدیل شده
