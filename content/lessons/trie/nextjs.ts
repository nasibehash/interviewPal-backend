// app/api/suggest/route.ts
import { NextRequest, NextResponse } from 'next/server';

interface TrieNode {
  children: Map<string, TrieNode>;
  isWord: boolean;
}

const normalize = (text: string): string => text.trim().toLowerCase().replaceAll('ي', 'ی').replaceAll('ك', 'ک');

// درخت یک بار موقع لود ماژول ساخته و بین درخواست‌ها به اشتراک گذاشته می‌شود
const root: TrieNode = { children: new Map(), isWord: false };
for (const word of ['گوشی', 'گوشی سامسونگ', 'گوشی شیائومی', 'گوشواره', 'لپ‌تاپ']) {
  let node = root;
  for (const char of normalize(word)) {
    let next = node.children.get(char);
    if (!next) node.children.set(char, (next = { children: new Map(), isWord: false }));
    node = next;
  }
  node.isWord = true;
}

function suggest(prefix: string, limit: number): string[] {
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

// GET /api/suggest?q=گوش
export function GET(request: NextRequest) {
  const q = request.nextUrl.searchParams.get('q') ?? '';
  if (q.length === 0 || q.length > 50) {
    return NextResponse.json({ error: 'q باید بین ۱ تا ۵۰ کاراکتر باشد' }, { status: 400 });
  }
  return NextResponse.json(suggest(q, 5));
}
