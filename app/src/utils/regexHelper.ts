export const emailReg = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/
export const domainReg = /^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$/
export const portReg = /^([1-9]|[1-9]\d{1,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$/
export const subDomainPrefix = /^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9])$/

export const passReg = /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,}$/
export const userNameReg = /^[a-zA-Z0-9_]{3,}$/


export function checkPass(str: string) {
  return passReg.test(str)
}
export function checkUserName(str: string) {
  return userNameReg.test(str)
}


export function checkEmail(str: string) {
  return emailReg.test(str)
}

export function checkDomain(str: string) {
  return domainReg.test(str)
}
export function checkPort(str: string) {
  return portReg.test(str)
}

export function checksubDomainPrefix(str: string) {
  if (!str || str === '')
    return false;
  const subs = str.split(';')
  if (subs) {
    let succ = true
    subs.forEach(sub => {
      if (sub && sub != '')
        if (!subDomainPrefix.test(sub)) {
          succ = false;
        }
    });
    return succ
  } else {
    return false;
  }
}




