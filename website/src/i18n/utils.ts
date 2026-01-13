import { ui, defaultLang, type UIKey } from './ui';

export type Lang = keyof typeof ui;

export function getLangFromUrl(url: URL): Lang {
  const base = import.meta.env.BASE_URL || '/';
  // Remove base path from pathname
  let pathname = url.pathname;
  if (base !== '/' && pathname.startsWith(base)) {
    pathname = pathname.slice(base.length);
  }
  // Get the first segment after base
  const [, lang] = pathname.split('/');
  if (lang && lang in ui) return lang as Lang;
  return defaultLang;
}

export function useTranslations(lang: Lang) {
  return function t(key: UIKey): string {
    return ui[lang][key] || ui[defaultLang][key];
  };
}

export function getLocalePath(lang: Lang, path: string = ''): string {
  const base = import.meta.env.BASE_URL || '/';
  if (lang === defaultLang) {
    return `${base}${path}`.replace(/\/+/g, '/');
  }
  return `${base}${lang}/${path}`.replace(/\/+/g, '/');
}
