#use FIRST 4 digits of commit (gdi) 
#gdin -- diff only #filenames 
#glf fil -- show history for fil 
#glfp fil -- show FULL history for fil 
#gk fil -- show #graphical history for fil 
#
alias ghe='more ~/.bash_aliases '
alias gs='git status'
alias gi='git init '
alias gcl='git clone '
alias ga='git add'
alias gc='git commit '
alias gac='git commit -a  '
alias gca='git commit --amend'
alias gch='git checkout '
alias gb='git branch '
alias gdi='git diff '
alias gm='git merge'
alias gdin='git diff --name-only'
alias gt='git tag '
alias gl='git log --decorate --graph --all'
alias glo='git log --decorate --graph --all --oneline'
alias glf='git log -- '
alias glfp='git log -p -- '
alias grh='git reset HEAD --hard'
alias gdt='git difftool '
alias gpl='git pull '
alias gps='git push origin master --tags'
alias gcc='git config credential.helper store '
alias gf='git fetch '
alias gdm='git diff master origin/master'
alias gk='gitk'
alias gsync='gpl;gps'

alias sl=' . ~/.bashrc '
