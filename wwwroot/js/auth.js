window.apiPostWithCredentials = async function(url, obj) {
  const res = await fetch(url, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(obj)
  });
  const text = await res.text();
  let json = null;
  try { json = JSON.parse(text); } catch { json = null; }
  return { ok: res.ok, status: res.status, json: json, text: text };
};

window.submitLoginForm = function(email, password, rememberMe, returnUrl) {
  const f = document.createElement('form');
  f.method = 'post';
  f.action = '/api/account/login-form';
  function add(name, value) { const i = document.createElement('input'); i.type = 'hidden'; i.name = name; i.value = value ?? ''; f.appendChild(i); }
  add('Email', email);
  add('Password', password);
  add('RememberMe', rememberMe ? 'true' : 'false');
  if (returnUrl) add('ReturnUrl', returnUrl);
  document.body.appendChild(f);
  f.submit();
};
