import { useMemo, useState } from 'react';

interface TrieNode {
  children: Map<string, TrieNode>;
  isWord: boolean;
}

const normalize = (text: string): string => text.trim().toLowerCase().replaceAll('ي', 'ی').replaceAll('ك', 'ک');

function buildTrie(words: readonly string[]): TrieNode {
  const root: TrieNode = { children: new Map(), isWord: false };
  for (const word of words) {
    let node = root;
    for (const char of normalize(word)) {
      let next = node.children.get(char);
      if (!next) node.children.set(char, (next = { children: new Map(), isWord: false }));
      node = next;
    }
    node.isWord = true;
  }
  return root;
}

function suggest(root: TrieNode, prefix: string, limit: number): string[] {
  const typed = normalize(prefix);
  let node: TrieNode | undefined = root;
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

export function useAutocomplete(words: readonly string[], limit = 5) {
  const [query, setQuery] = useState('');
  const trie = useMemo(() => buildTrie(words), [words]); // فقط وقتی فهرست کلمه‌ها عوض شود
  const suggestions = useMemo(() => (query ? suggest(trie, query, limit) : []), [trie, query, limit]);
  return { query, setQuery, suggestions };
}

export function SearchBox({ products }: { products: readonly string[] }) {
  const { query, setQuery, suggestions } = useAutocomplete(products);
  return (
    <>
      <input type="search" role="combobox" aria-label="جست‌وجو" value={query} onChange={(e) => setQuery(e.target.value)} />
      <ul>
        {suggestions.map((s) => (
          <li key={s}><button onClick={() => setQuery(s)}>{s}</button></li>
        ))}
      </ul>
    </>
  );
}
